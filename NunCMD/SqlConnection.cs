#region Assembly System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Data.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.SqlServer.Server;

namespace System.Data.SqlClient;

//
// Summary:
//     Represents a connection to a SQL Server database. This class cannot be inherited.
[DefaultEvent("InfoMessage")]
public sealed class SqlConnection : DbConnection, ICloneable
{
    private class OpenAsyncRetry
    {
        private SqlConnection _parent;

        private TaskCompletionSource<DbConnectionInternal> _retry;

        private TaskCompletionSource<object> _result;

        private CancellationTokenRegistration _registration;

        public OpenAsyncRetry(SqlConnection parent, TaskCompletionSource<DbConnectionInternal> retry, TaskCompletionSource<object> result, CancellationTokenRegistration registration)
        {
            _parent = parent;
            _retry = retry;
            _result = result;
            _registration = registration;
        }

        internal void Retry(Task<DbConnectionInternal> retryTask)
        {
            Bid.Trace("<sc.SqlConnection.OpenAsyncRetry|Info> %d#\n", _parent.ObjectID);
            _registration.Dispose();
            try
            {
                SqlStatistics statistics = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    statistics = SqlStatistics.StartTimer(_parent.Statistics);
                    if (retryTask.IsFaulted)
                    {
                        Exception innerException = retryTask.Exception.InnerException;
                        _parent.CloseInnerConnection();
                        _parent._currentCompletion = null;
                        _result.SetException(retryTask.Exception.InnerException);
                        return;
                    }

                    if (retryTask.IsCanceled)
                    {
                        _parent.CloseInnerConnection();
                        _parent._currentCompletion = null;
                        _result.SetCanceled();
                        return;
                    }

                    bool flag;
                    lock (_parent.InnerConnection)
                    {
                        flag = _parent.TryOpen(_retry);
                    }

                    if (flag)
                    {
                        _parent._currentCompletion = null;
                        _result.SetResult(null);
                    }
                    else
                    {
                        _parent.CloseInnerConnection();
                        _parent._currentCompletion = null;
                        _result.SetException(ADP.ExceptionWithStackTrace(ADP.InternalError(ADP.InternalErrorCode.CompletedConnectReturnedPending)));
                    }
                }
                finally
                {
                    SqlStatistics.StopTimer(statistics);
                }
            }
            catch (Exception exception)
            {
                _parent.CloseInnerConnection();
                _parent._currentCompletion = null;
                _result.SetException(exception);
            }
        }
    }

    private static readonly object EventInfoMessage;

    internal static readonly SqlColumnEncryptionEnclaveProviderConfigurationManager sqlColumnEncryptionEnclaveProviderConfigurationManager;

    private static readonly Dictionary<string, SqlColumnEncryptionKeyStoreProvider> _SystemColumnEncryptionKeyStoreProviders;

    private static ReadOnlyDictionary<string, SqlColumnEncryptionKeyStoreProvider> _CustomColumnEncryptionKeyStoreProviders;

    private static readonly object _CustomColumnEncryptionKeyProvidersLock;

    private static readonly ConcurrentDictionary<string, IList<string>> _ColumnEncryptionTrustedMasterKeyPaths;

    private static bool _ColumnEncryptionQueryMetadataCacheEnabled;

    private static TimeSpan _ColumnEncryptionKeyCacheTtl;

    private SqlDebugContext _sdc;

    private bool _AsyncCommandInProgress;

    internal SqlStatistics _statistics;

    private bool _collectstats;

    private bool _fireInfoMessageEventOnUserErrors;

    private Tuple<TaskCompletionSource<DbConnectionInternal>, Task> _currentCompletion;

    private SqlCredential _credential;

    private string _connectionString;

    private int _connectRetryCount;

    private string _accessToken;

    private object _reconnectLock = new object();

    internal Task _currentReconnectionTask;

    private Task _asyncWaitingForReconnection;

    private Guid _originalConnectionId = Guid.Empty;

    private CancellationTokenSource _reconnectionCancellationSource;

    internal SessionData _recoverySessionData;

    internal WindowsIdentity _lastIdentity;

    internal WindowsIdentity _impersonateIdentity;

    private int _reconnectCount;

    internal bool _applyTransientFaultHandling;

    private static readonly DbConnectionFactory _connectionFactory;

    internal static readonly CodeAccessPermission ExecutePermission;

    private DbConnectionOptions _userConnectionOptions;

    private DbConnectionPoolGroup _poolGroup;

    private DbConnectionInternal _innerConnection;

    private int _closeCount;

    private static int _objectTypeCount;

    internal readonly int ObjectID = Interlocked.Increment(ref _objectTypeCount);

    //
    // Summary:
    //     Allows you to set a list of trusted key paths for a database server. If while
    //     processing an application query the driver receives a key path that is not on
    //     the list, the query will fail. This property provides additional protection against
    //     security attacks that involve a compromised SQL Server providing fake key paths,
    //     which may lead to leaking key store credentials.
    //
    // Returns:
    //     The list of trusted master key paths for the column encryption.
    [DefaultValue(null)]
    [ResCategory("DataCategory_Data")]
    [ResDescription("TCE_SqlConnection_TrustedColumnMasterKeyPaths")]
    public static IDictionary<string, IList<string>> ColumnEncryptionTrustedMasterKeyPaths => _ColumnEncryptionTrustedMasterKeyPaths;

    //
    // Summary:
    //     Gets or sets a value that indicates whether query metadata caching is enabled
    //     (true) or not (false) for parameterized queries running against Always Encrypted
    //     enabled databases. The default value is true.
    //
    // Returns:
    //     Returns true if query metadata caching is enabled; otherwise false. true is the
    //     default.
    [DefaultValue(null)]
    [ResCategory("DataCategory_Data")]
    [ResDescription("TCE_SqlConnection_ColumnEncryptionQueryMetadataCacheEnabled")]
    public static bool ColumnEncryptionQueryMetadataCacheEnabled
    {
        get
        {
            return _ColumnEncryptionQueryMetadataCacheEnabled;
        }
        set
        {
            _ColumnEncryptionQueryMetadataCacheEnabled = value;
        }
    }

    //
    // Summary:
    //     Gets or sets the time-to-live for column encryption key entries in the column
    //     encryption key cache for the Always Encrypted feature. The default value is 2
    //     hours. 0 means no caching at all.
    //
    // Returns:
    //     The time interval.
    [ResDescription("TCE_SqlConnection_ColumnEncryptionKeyCacheTtl")]
    [DefaultValue(null)]
    [ResCategory("DataCategory_Data")]
    public static TimeSpan ColumnEncryptionKeyCacheTtl
    {
        get
        {
            return _ColumnEncryptionKeyCacheTtl;
        }
        set
        {
            _ColumnEncryptionKeyCacheTtl = value;
        }
    }

    //
    // Summary:
    //     When set to true, enables statistics gathering for the current connection.
    //
    // Returns:
    //     Returns true if statistics gathering is enabled; otherwise false. false is the
    //     default.
    [DefaultValue(false)]
    [ResDescription("SqlConnection_StatisticsEnabled")]
    [ResCategory("DataCategory_Data")]
    public bool StatisticsEnabled
    {
        get
        {
            return _collectstats;
        }
        set
        {
            if (IsContextConnection)
            {
                if (value)
                {
                    throw SQL.NotAvailableOnContextConnection();
                }

                return;
            }

            if (value)
            {
                if (ConnectionState.Open == State)
                {
                    if (_statistics == null)
                    {
                        _statistics = new SqlStatistics();
                        ADP.TimerCurrent(out _statistics._openTimestamp);
                    }

                    Parser.Statistics = _statistics;
                }
            }
            else if (_statistics != null && ConnectionState.Open == State)
            {
                TdsParser parser = Parser;
                parser.Statistics = null;
                ADP.TimerCurrent(out _statistics._closeTimestamp);
            }

            _collectstats = value;
        }
    }

    internal bool AsyncCommandInProgress
    {
        get
        {
            return _AsyncCommandInProgress;
        }
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        set
        {
            _AsyncCommandInProgress = value;
        }
    }

    internal bool IsContextConnection
    {
        get
        {
            SqlConnectionString opt = (SqlConnectionString)ConnectionOptions;
            return UsesContextConnection(opt);
        }
    }

    internal bool IsColumnEncryptionSettingEnabled
    {
        get
        {
            SqlConnectionString sqlConnectionString = (SqlConnectionString)ConnectionOptions;
            if (sqlConnectionString == null)
            {
                return false;
            }

            return sqlConnectionString.ColumnEncryptionSetting == SqlConnectionColumnEncryptionSetting.Enabled;
        }
    }

    internal string EnclaveAttestationUrl
    {
        get
        {
            SqlConnectionString sqlConnectionString = (SqlConnectionString)ConnectionOptions;
            return sqlConnectionString.EnclaveAttestationUrl;
        }
    }

    internal SqlConnectionString.TransactionBindingEnum TransactionBinding => ((SqlConnectionString)ConnectionOptions).TransactionBinding;

    internal SqlConnectionString.TypeSystem TypeSystem => ((SqlConnectionString)ConnectionOptions).TypeSystemVersion;

    internal Version TypeSystemAssemblyVersion => ((SqlConnectionString)ConnectionOptions).TypeSystemAssemblyVersion;

    internal PoolBlockingPeriod PoolBlockingPeriod => ((SqlConnectionString)ConnectionOptions).PoolBlockingPeriod;

    internal int ConnectRetryInterval => ((SqlConnectionString)ConnectionOptions).ConnectRetryInterval;

    protected override DbProviderFactory DbProviderFactory => SqlClientFactory.Instance;

    //
    // Summary:
    //     Gets or sets the access token for the connection.
    //
    // Returns:
    //     The access token for the connection.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ResDescription("SqlConnection_AccessToken")]
    [Browsable(false)]
    public string AccessToken
    {
        get
        {
            string result = _accessToken;
            SqlConnectionString sqlConnectionString = (SqlConnectionString)UserConnectionOptions;
            if (InnerConnection.ShouldHidePassword && sqlConnectionString != null && !sqlConnectionString.PersistSecurityInfo)
            {
                result = null;
            }

            return result;
        }
        set
        {
            if (!InnerConnection.AllowSetConnectionString)
            {
                throw ADP.OpenConnectionPropertySet("AccessToken", InnerConnection.State);
            }

            if (value != null)
            {
                CheckAndThrowOnInvalidCombinationOfConnectionOptionAndAccessToken((SqlConnectionString)ConnectionOptions);
            }

            _accessToken = value;
            ConnectionString_Set(new SqlConnectionPoolKey(_connectionString, _credential, _accessToken));
        }
    }

    //
    // Summary:
    //     Gets or sets the string used to open a SQL Server database.
    //
    // Returns:
    //     The connection string that includes the source database name, and other parameters
    //     needed to establish the initial connection. The default value is an empty string.
    //
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     An invalid connection string argument has been supplied, or a required connection
    //     string argument has not been supplied.
    [ResDescription("SqlConnection_ConnectionString")]
    [DefaultValue("")]
    [RecommendedAsConfigurable(true)]
    [SettingsBindable(true)]
    [RefreshProperties(RefreshProperties.All)]
    [ResCategory("DataCategory_Data")]
    [Editor("Microsoft.VSDesigner.Data.SQL.Design.SqlConnectionStringEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public override string ConnectionString
    {
        get
        {
            return ConnectionString_Get();
        }
        set
        {
            if (_credential != null || _accessToken != null)
            {
                SqlConnectionString sqlConnectionString = new SqlConnectionString(value);
                if (_credential != null)
                {
                    if (UsesActiveDirectoryIntegrated(sqlConnectionString))
                    {
                        throw SQL.SettingIntegratedWithCredential();
                    }

                    CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential(sqlConnectionString);
                }
                else if (_accessToken != null)
                {
                    CheckAndThrowOnInvalidCombinationOfConnectionOptionAndAccessToken(sqlConnectionString);
                }
            }

            ConnectionString_Set(new SqlConnectionPoolKey(value, _credential, _accessToken));
            _connectionString = value;
            CacheConnectionStringProperties();
        }
    }

    //
    // Summary:
    //     Gets the time to wait while trying to establish a connection before terminating
    //     the attempt and generating an error.
    //
    // Returns:
    //     The time (in seconds) to wait for a connection to open. The default value is
    //     15 seconds.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     The value set is less than 0.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ResDescription("SqlConnection_ConnectionTimeout")]
    public override int ConnectionTimeout => ((SqlConnectionString)ConnectionOptions)?.ConnectTimeout ?? 15;

    //
    // Summary:
    //     Gets the name of the current database or the database to be used after a connection
    //     is opened.
    //
    // Returns:
    //     The name of the current database or the name of the database to be used after
    //     a connection is opened. The default value is an empty string.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ResDescription("SqlConnection_Database")]
    public override string Database
    {
        get
        {
            if (InnerConnection is SqlInternalConnection sqlInternalConnection)
            {
                return sqlInternalConnection.CurrentDatabase;
            }

            SqlConnectionString sqlConnectionString = (SqlConnectionString)ConnectionOptions;
            return (sqlConnectionString != null) ? sqlConnectionString.InitialCatalog : "";
        }
    }

    //
    // Summary:
    //     Gets the name of the instance of SQL Server to which to connect.
    //
    // Returns:
    //     The name of the instance of SQL Server to which to connect. The default value
    //     is an empty string.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ResDescription("SqlConnection_DataSource")]
    [Browsable(true)]
    public override string DataSource
    {
        get
        {
            if (InnerConnection is SqlInternalConnection sqlInternalConnection)
            {
                return sqlInternalConnection.CurrentDataSource;
            }

            SqlConnectionString sqlConnectionString = (SqlConnectionString)ConnectionOptions;
            return (sqlConnectionString != null) ? sqlConnectionString.DataSource : "";
        }
    }

    //
    // Summary:
    //     Gets the size (in bytes) of network packets used to communicate with an instance
    //     of SQL Server.
    //
    // Returns:
    //     The size (in bytes) of network packets. The default value is 8000.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ResCategory("DataCategory_Data")]
    [ResDescription("SqlConnection_PacketSize")]
    public int PacketSize
    {
        get
        {
            if (IsContextConnection)
            {
                throw SQL.NotAvailableOnContextConnection();
            }

            if (InnerConnection is SqlInternalConnectionTds sqlInternalConnectionTds)
            {
                return sqlInternalConnectionTds.PacketSize;
            }

            return ((SqlConnectionString)ConnectionOptions)?.PacketSize ?? 8000;
        }
    }

    //
    // Summary:
    //     The connection ID of the most recent connection attempt, regardless of whether
    //     the attempt succeeded or failed.
    //
    // Returns:
    //     The connection ID of the most recent connection attempt.
    [ResDescription("SqlConnection_ClientConnectionId")]
    [ResCategory("DataCategory_Data")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Guid ClientConnectionId
    {
        get
        {
            if (InnerConnection is SqlInternalConnectionTds sqlInternalConnectionTds)
            {
                return sqlInternalConnectionTds.ClientConnectionId;
            }

            Task currentReconnectionTask = _currentReconnectionTask;
            if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
            {
                return _originalConnectionId;
            }

            return Guid.Empty;
        }
    }

    //
    // Summary:
    //     Gets a string that contains the version of the instance of SQL Server to which
    //     the client is connected.
    //
    // Returns:
    //     The version of the instance of SQL Server.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     The connection is closed. System.Data.SqlClient.SqlConnection.ServerVersion was
    //     called while the returned Task was not completed and the connection was not opened
    //     after a call to System.Data.SqlClient.SqlConnection.OpenAsync(System.Threading.CancellationToken).
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    [ResDescription("SqlConnection_ServerVersion")]
    public override string ServerVersion => GetOpenConnection().ServerVersion;

    //
    // Summary:
    //     Indicates the state of the System.Data.SqlClient.SqlConnection during the most
    //     recent network operation performed on the connection.
    //
    // Returns:
    //     An System.Data.ConnectionState enumeration.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ResDescription("DbConnection_State")]
    [Browsable(false)]
    public override ConnectionState State
    {
        get
        {
            Task currentReconnectionTask = _currentReconnectionTask;
            if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
            {
                return ConnectionState.Open;
            }

            return InnerConnection.State;
        }
    }

    internal SqlStatistics Statistics => _statistics;

    //
    // Summary:
    //     Gets a string that identifies the database client.
    //
    // Returns:
    //     A string that identifies the database client. If not specified, the name of the
    //     client computer. If neither is specified, the value is an empty string.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ResCategory("DataCategory_Data")]
    [ResDescription("SqlConnection_WorkstationId")]
    public string WorkstationId
    {
        get
        {
            if (IsContextConnection)
            {
                throw SQL.NotAvailableOnContextConnection();
            }

            string text = ((SqlConnectionString)ConnectionOptions)?.WorkstationId;
            if (text == null)
            {
                text = Environment.MachineName;
            }

            return text;
        }
    }

    //
    // Summary:
    //     Gets or sets the System.Data.SqlClient.SqlCredential object for this connection.
    //
    //
    // Returns:
    //     The System.Data.SqlClient.SqlCredential object for this connection.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [ResDescription("SqlConnection_Credential")]
    [Browsable(false)]
    public SqlCredential Credential
    {
        get
        {
            SqlCredential result = _credential;
            SqlConnectionString sqlConnectionString = (SqlConnectionString)UserConnectionOptions;
            if (InnerConnection.ShouldHidePassword && sqlConnectionString != null && !sqlConnectionString.PersistSecurityInfo)
            {
                result = null;
            }

            return result;
        }
        set
        {
            if (!InnerConnection.AllowSetConnectionString)
            {
                throw ADP.OpenConnectionPropertySet("Credential", InnerConnection.State);
            }

            if (value != null)
            {
                if (UsesActiveDirectoryIntegrated((SqlConnectionString)ConnectionOptions))
                {
                    throw SQL.SettingCredentialWithIntegratedInvalid();
                }

                CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential((SqlConnectionString)ConnectionOptions);
                if (_accessToken != null)
                {
                    throw ADP.InvalidMixedUsageOfCredentialAndAccessToken();
                }
            }

            _credential = value;
            ConnectionString_Set(new SqlConnectionPoolKey(_connectionString, _credential, _accessToken));
        }
    }

    //
    // Summary:
    //     Gets or sets the System.Data.SqlClient.SqlConnection.FireInfoMessageEventOnUserErrors
    //     property.
    //
    // Returns:
    //     true if the System.Data.SqlClient.SqlConnection.FireInfoMessageEventOnUserErrors
    //     property has been set; otherwise false.
    public bool FireInfoMessageEventOnUserErrors
    {
        get
        {
            return _fireInfoMessageEventOnUserErrors;
        }
        set
        {
            _fireInfoMessageEventOnUserErrors = value;
        }
    }

    internal int ReconnectCount => _reconnectCount;

    internal bool HasLocalTransaction => GetOpenConnection().HasLocalTransaction;

    internal bool HasLocalTransactionFromAPI
    {
        get
        {
            Task currentReconnectionTask = _currentReconnectionTask;
            if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
            {
                return false;
            }

            return GetOpenConnection().HasLocalTransactionFromAPI;
        }
    }

    internal bool IsShiloh
    {
        get
        {
            if (_currentReconnectionTask != null)
            {
                return true;
            }

            return GetOpenConnection().IsShiloh;
        }
    }

    internal bool IsYukonOrNewer
    {
        get
        {
            if (_currentReconnectionTask != null)
            {
                return true;
            }

            return GetOpenConnection().IsYukonOrNewer;
        }
    }

    internal bool IsKatmaiOrNewer
    {
        get
        {
            if (_currentReconnectionTask != null)
            {
                return true;
            }

            return GetOpenConnection().IsKatmaiOrNewer;
        }
    }

    internal TdsParser Parser
    {
        get
        {
            if (!(GetOpenConnection() is SqlInternalConnectionTds sqlInternalConnectionTds))
            {
                throw SQL.NotAvailableOnContextConnection();
            }

            return sqlInternalConnectionTds.Parser;
        }
    }

    internal bool Asynchronous => ((SqlConnectionString)ConnectionOptions)?.Asynchronous ?? false;

    internal int CloseCount => _closeCount;

    internal DbConnectionFactory ConnectionFactory => _connectionFactory;

    internal DbConnectionOptions ConnectionOptions => PoolGroup?.ConnectionOptions;

    internal DbConnectionInternal InnerConnection => _innerConnection;

    internal DbConnectionPoolGroup PoolGroup
    {
        get
        {
            return _poolGroup;
        }
        set
        {
            _poolGroup = value;
        }
    }

    internal DbConnectionOptions UserConnectionOptions => _userConnectionOptions;

    //
    // Summary:
    //     Occurs when SQL Server returns a warning or informational message.
    [ResCategory("DataCategory_InfoMessage")]
    [ResDescription("DbConnection_InfoMessage")]
    public event SqlInfoMessageEventHandler InfoMessage
    {
        add
        {
            base.Events.AddHandler(EventInfoMessage, value);
        }
        remove
        {
            base.Events.RemoveHandler(EventInfoMessage, value);
        }
    }

    static SqlConnection()
    {
        //IL_00ad: Expected O, but got Unknown
        EventInfoMessage = new object();
        _SystemColumnEncryptionKeyStoreProviders = new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>(1, StringComparer.OrdinalIgnoreCase)
        {
            {
                "MSSQL_CERTIFICATE_STORE",
                new SqlColumnEncryptionCertificateStoreProvider()
            },
            {
                "MSSQL_CNG_STORE",
                new SqlColumnEncryptionCngProvider()
            },
            {
                "MSSQL_CSP_PROVIDER",
                new SqlColumnEncryptionCspProvider()
            }
        };
        _CustomColumnEncryptionKeyProvidersLock = new object();
        _ColumnEncryptionTrustedMasterKeyPaths = new ConcurrentDictionary<string, IList<string>>(4 * Environment.ProcessorCount, 1, StringComparer.OrdinalIgnoreCase);
        _ColumnEncryptionQueryMetadataCacheEnabled = true;
        _ColumnEncryptionKeyCacheTtl = TimeSpan.FromHours(2.0);
        _connectionFactory = SqlConnectionFactory.SingletonInstance;
        ExecutePermission = CreateExecutePermission();
        SqlColumnEncryptionEnclaveProviderConfigurationSection sqlColumnEncryptionEnclaveProviderConfigurationSection = null;
        try
        {
            sqlColumnEncryptionEnclaveProviderConfigurationSection = (SqlColumnEncryptionEnclaveProviderConfigurationSection)ConfigurationManager.GetSection("SqlColumnEncryptionEnclaveProviders");
        }
        catch (ConfigurationErrorsException val)
        {
            ConfigurationErrorsException innerException = val;
            throw SQL.CannotGetSqlColumnEncryptionEnclaveProviderConfig((Exception)(object)innerException);
        }

        sqlColumnEncryptionEnclaveProviderConfigurationManager = new SqlColumnEncryptionEnclaveProviderConfigurationManager(sqlColumnEncryptionEnclaveProviderConfigurationSection);
    }

    //
    // Summary:
    //     Registers the column encryption key store providers.
    //
    // Parameters:
    //   customProviders:
    //     The custom providers
    public static void RegisterColumnEncryptionKeyStoreProviders(IDictionary<string, SqlColumnEncryptionKeyStoreProvider> customProviders)
    {
        if (customProviders == null)
        {
            throw SQL.NullCustomKeyStoreProviderDictionary();
        }

        foreach (string key in customProviders.Keys)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw SQL.EmptyProviderName();
            }

            if (key.StartsWith("MSSQL_", StringComparison.InvariantCultureIgnoreCase))
            {
                throw SQL.InvalidCustomKeyStoreProviderName(key, "MSSQL_");
            }

            if (customProviders[key] == null)
            {
                throw SQL.NullProviderValue(key);
            }
        }

        lock (_CustomColumnEncryptionKeyProvidersLock)
        {
            if (_CustomColumnEncryptionKeyStoreProviders != null)
            {
                throw SQL.CanOnlyCallOnce();
            }

            Dictionary<string, SqlColumnEncryptionKeyStoreProvider> dictionary = new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>(customProviders, StringComparer.OrdinalIgnoreCase);
            _CustomColumnEncryptionKeyStoreProviders = new ReadOnlyDictionary<string, SqlColumnEncryptionKeyStoreProvider>(dictionary);
        }
    }

    internal static bool TryGetColumnEncryptionKeyStoreProvider(string providerName, out SqlColumnEncryptionKeyStoreProvider columnKeyStoreProvider)
    {
        columnKeyStoreProvider = null;
        if (_SystemColumnEncryptionKeyStoreProviders.TryGetValue(providerName, out columnKeyStoreProvider))
        {
            return true;
        }

        lock (_CustomColumnEncryptionKeyProvidersLock)
        {
            if (_CustomColumnEncryptionKeyStoreProviders == null)
            {
                return false;
            }

            return _CustomColumnEncryptionKeyStoreProviders.TryGetValue(providerName, out columnKeyStoreProvider);
        }
    }

    internal static List<string> GetColumnEncryptionSystemKeyStoreProviders()
    {
        HashSet<string> source = new HashSet<string>(_SystemColumnEncryptionKeyStoreProviders.Keys);
        return source.ToList();
    }

    internal static List<string> GetColumnEncryptionCustomKeyStoreProviders()
    {
        if (_CustomColumnEncryptionKeyStoreProviders != null)
        {
            HashSet<string> source = new HashSet<string>(_CustomColumnEncryptionKeyStoreProviders.Keys);
            return source.ToList();
        }

        return new List<string>();
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Data.SqlClient.SqlConnection class when
    //     given a string that contains the connection string.
    //
    // Parameters:
    //   connectionString:
    //     The connection used to open the SQL Server database.
    public SqlConnection(string connectionString)
        : this(connectionString, null)
    {
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Data.SqlClient.SqlConnection class given
    //     a connection string, that does not use Integrated Security = true and a System.Data.SqlClient.SqlCredential
    //     object that contains the user ID and password.
    //
    // Parameters:
    //   connectionString:
    //     A connection string that does not use any of the following connection string
    //     keywords: Integrated Security = true, UserId, or Password; or that does not use
    //     ContextConnection = true.
    //
    //   credential:
    //     A System.Data.SqlClient.SqlCredential object. If credential is null, System.Data.SqlClient.SqlConnection.#ctor(System.String,System.Data.SqlClient.SqlCredential)
    //     is functionally equivalent to System.Data.SqlClient.SqlConnection.#ctor(System.String).
    public SqlConnection(string connectionString, SqlCredential credential)
        : this()
    {
        ConnectionString = connectionString;
        if (credential != null)
        {
            SqlConnectionString opt = (SqlConnectionString)ConnectionOptions;
            if (UsesClearUserIdOrPassword(opt))
            {
                throw ADP.InvalidMixedArgumentOfSecureAndClearCredential();
            }

            if (UsesIntegratedSecurity(opt))
            {
                throw ADP.InvalidMixedArgumentOfSecureCredentialAndIntegratedSecurity();
            }

            if (UsesContextConnection(opt))
            {
                throw ADP.InvalidMixedArgumentOfSecureCredentialAndContextConnection();
            }

            if (UsesActiveDirectoryIntegrated(opt))
            {
                throw SQL.SettingCredentialWithIntegratedArgument();
            }

            Credential = credential;
        }

        CacheConnectionStringProperties();
    }

    private SqlConnection(SqlConnection connection)
    {
        GC.SuppressFinalize(this);
        CopyFrom(connection);
        _connectionString = connection._connectionString;
        if (connection._credential != null)
        {
            SecureString secureString = connection._credential.Password.Copy();
            secureString.MakeReadOnly();
            _credential = new SqlCredential(connection._credential.UserId, secureString);
        }

        _accessToken = connection._accessToken;
        CacheConnectionStringProperties();
    }

    private void CacheConnectionStringProperties()
    {
        if (ConnectionOptions is SqlConnectionString sqlConnectionString)
        {
            _connectRetryCount = sqlConnectionString.ConnectRetryCount;
            if (_connectRetryCount == 1 && ADP.IsAzureSqlServerEndpoint(sqlConnectionString.DataSource))
            {
                _connectRetryCount = 2;
            }
        }
    }

    private bool UsesContextConnection(SqlConnectionString opt)
    {
        return opt?.ContextConnection ?? false;
    }

    private bool UsesActiveDirectoryIntegrated(SqlConnectionString opt)
    {
        if (opt == null)
        {
            return false;
        }

        return opt.Authentication == SqlAuthenticationMethod.ActiveDirectoryIntegrated;
    }

    private bool UsesAuthentication(SqlConnectionString opt)
    {
        if (opt == null)
        {
            return false;
        }

        return opt.Authentication != SqlAuthenticationMethod.NotSpecified;
    }

    private bool UsesIntegratedSecurity(SqlConnectionString opt)
    {
        return opt?.IntegratedSecurity ?? false;
    }

    private bool UsesClearUserIdOrPassword(SqlConnectionString opt)
    {
        bool result = false;
        if (opt != null)
        {
            result = !ADP.IsEmpty(opt.UserID) || !ADP.IsEmpty(opt.Password);
        }

        return result;
    }

    private void CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential(SqlConnectionString connectionOptions)
    {
        if (UsesClearUserIdOrPassword(connectionOptions))
        {
            throw ADP.InvalidMixedUsageOfSecureAndClearCredential();
        }

        if (UsesIntegratedSecurity(connectionOptions))
        {
            throw ADP.InvalidMixedUsageOfSecureCredentialAndIntegratedSecurity();
        }

        if (UsesContextConnection(connectionOptions))
        {
            throw ADP.InvalidMixedArgumentOfSecureCredentialAndContextConnection();
        }
    }

    private void CheckAndThrowOnInvalidCombinationOfConnectionOptionAndAccessToken(SqlConnectionString connectionOptions)
    {
        if (UsesClearUserIdOrPassword(connectionOptions))
        {
            throw ADP.InvalidMixedUsageOfAccessTokenAndUserIDPassword();
        }

        if (UsesIntegratedSecurity(connectionOptions))
        {
            throw ADP.InvalidMixedUsageOfAccessTokenAndIntegratedSecurity();
        }

        if (UsesContextConnection(connectionOptions))
        {
            throw ADP.InvalidMixedUsageOfAccessTokenAndContextConnection();
        }

        if (UsesAuthentication(connectionOptions))
        {
            throw ADP.InvalidMixedUsageOfAccessTokenAndAuthentication();
        }

        if (_credential != null)
        {
            throw ADP.InvalidMixedUsageOfAccessTokenAndCredential();
        }
    }

    //
    // Summary:
    //     Starts a database transaction.
    //
    // Returns:
    //     An object representing the new transaction.
    //
    // Exceptions:
    //   T:System.Data.SqlClient.SqlException:
    //     Parallel transactions are not allowed when using Multiple Active Result Sets
    //     (MARS).
    //
    //   T:System.InvalidOperationException:
    //     Parallel transactions are not supported.
    public new SqlTransaction BeginTransaction()
    {
        return BeginTransaction(IsolationLevel.Unspecified, null);
    }

    //
    // Summary:
    //     Starts a database transaction with the specified isolation level.
    //
    // Parameters:
    //   iso:
    //     The isolation level under which the transaction should run.
    //
    // Returns:
    //     An object representing the new transaction.
    //
    // Exceptions:
    //   T:System.Data.SqlClient.SqlException:
    //     Parallel transactions are not allowed when using Multiple Active Result Sets
    //     (MARS).
    //
    //   T:System.InvalidOperationException:
    //     Parallel transactions are not supported.
    public new SqlTransaction BeginTransaction(IsolationLevel iso)
    {
        return BeginTransaction(iso, null);
    }

    //
    // Summary:
    //     Starts a database transaction with the specified transaction name.
    //
    // Parameters:
    //   transactionName:
    //     The name of the transaction.
    //
    // Returns:
    //     An object representing the new transaction.
    //
    // Exceptions:
    //   T:System.Data.SqlClient.SqlException:
    //     Parallel transactions are not allowed when using Multiple Active Result Sets
    //     (MARS).
    //
    //   T:System.InvalidOperationException:
    //     Parallel transactions are not supported.
    public SqlTransaction BeginTransaction(string transactionName)
    {
        return BeginTransaction(IsolationLevel.Unspecified, transactionName);
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        Bid.ScopeEnter(out var hScp, "<prov.SqlConnection.BeginDbTransaction|API> %d#, isolationLevel=%d{ds.IsolationLevel}", ObjectID, (int)isolationLevel);
        try
        {
            DbTransaction result = BeginTransaction(isolationLevel);
            GC.KeepAlive(this);
            return result;
        }
        finally
        {
            Bid.ScopeLeave(ref hScp);
        }
    }

    //
    // Summary:
    //     Starts a database transaction with the specified isolation level and transaction
    //     name.
    //
    // Parameters:
    //   iso:
    //     The isolation level under which the transaction should run.
    //
    //   transactionName:
    //     The name of the transaction.
    //
    // Returns:
    //     An object representing the new transaction.
    //
    // Exceptions:
    //   T:System.Data.SqlClient.SqlException:
    //     Parallel transactions are not allowed when using Multiple Active Result Sets
    //     (MARS).
    //
    //   T:System.InvalidOperationException:
    //     Parallel transactions are not supported.
    public SqlTransaction BeginTransaction(IsolationLevel iso, string transactionName)
    {
        WaitForPendingReconnection();
        SqlStatistics statistics = null;
        string a = (ADP.IsEmpty(transactionName) ? "None" : transactionName);
        Bid.ScopeEnter(out var hScp, "<sc.SqlConnection.BeginTransaction|API> %d#, iso=%d{ds.IsolationLevel}, transactionName='%ls'\n", ObjectID, (int)iso, a);
        try
        {
            statistics = SqlStatistics.StartTimer(Statistics);
            bool shouldReconnect = true;
            SqlTransaction sqlTransaction;
            do
            {
                sqlTransaction = GetOpenConnection().BeginSqlTransaction(iso, transactionName, shouldReconnect);
                shouldReconnect = false;
            }
            while (sqlTransaction.InternalTransaction.ConnectionHasBeenRestored);
            GC.KeepAlive(this);
            return sqlTransaction;
        }
        finally
        {
            Bid.ScopeLeave(ref hScp);
            SqlStatistics.StopTimer(statistics);
        }
    }

    //
    // Summary:
    //     Changes the current database for an open System.Data.SqlClient.SqlConnection.
    //
    //
    // Parameters:
    //   database:
    //     The name of the database to use instead of the current database.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     The database name is not valid.
    //
    //   T:System.InvalidOperationException:
    //     The connection is not open.
    //
    //   T:System.Data.SqlClient.SqlException:
    //     Cannot change the database.
    public override void ChangeDatabase(string database)
    {
        SqlStatistics statistics = null;
        RepairInnerConnection();
        Bid.CorrelationTrace("<sc.SqlConnection.ChangeDatabase|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
        TdsParser target = null;
        RuntimeHelpers.PrepareConstrainedRegions();
        try
        {
            target = SqlInternalConnection.GetBestEffortCleanupTarget(this);
            statistics = SqlStatistics.StartTimer(Statistics);
            InnerConnection.ChangeDatabase(database);
        }
        catch (OutOfMemoryException e)
        {
            Abort(e);
            throw;
        }
        catch (StackOverflowException e2)
        {
            Abort(e2);
            throw;
        }
        catch (ThreadAbortException e3)
        {
            Abort(e3);
            SqlInternalConnection.BestEffortCleanup(target);
            throw;
        }
        finally
        {
            SqlStatistics.StopTimer(statistics);
        }
    }

    //
    // Summary:
    //     Empties the connection pool.
    public static void ClearAllPools()
    {
        new SqlClientPermission(PermissionState.Unrestricted).Demand();
        SqlConnectionFactory.SingletonInstance.ClearAllPools();
    }

    //
    // Summary:
    //     Empties the connection pool associated with the specified connection.
    //
    // Parameters:
    //   connection:
    //     The System.Data.SqlClient.SqlConnection to be cleared from the pool.
    public static void ClearPool(SqlConnection connection)
    {
        ADP.CheckArgumentNull(connection, "connection");
        DbConnectionOptions userConnectionOptions = connection.UserConnectionOptions;
        if (userConnectionOptions != null)
        {
            userConnectionOptions.DemandPermission();
            if (connection.IsContextConnection)
            {
                throw SQL.NotAvailableOnContextConnection();
            }

            SqlConnectionFactory.SingletonInstance.ClearPool(connection);
        }
    }

    //
    // Summary:
    //     Creates a new object that is a copy of the current instance.
    //
    // Returns:
    //     A new object that is a copy of this instance.
    object ICloneable.Clone()
    {
        SqlConnection sqlConnection = new SqlConnection(this);
        Bid.Trace("<sc.SqlConnection.Clone|API> %d#, clone=%d#\n", ObjectID, sqlConnection.ObjectID);
        return sqlConnection;
    }

    private void CloseInnerConnection()
    {
        InnerConnection.CloseConnection(this, ConnectionFactory);
    }

    //
    // Summary:
    //     Closes the connection to the database. This is the preferred method of closing
    //     any open connection.
    //
    // Exceptions:
    //   T:System.Data.SqlClient.SqlException:
    //     The connection-level error that occurred while opening the connection.
    public override void Close()
    {
        Bid.ScopeEnter(out var hScp, "<sc.SqlConnection.Close|API> %d#", ObjectID);
        Bid.CorrelationTrace("<sc.SqlConnection.Close|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
        try
        {
            SqlStatistics statistics = null;
            TdsParser target = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                target = SqlInternalConnection.GetBestEffortCleanupTarget(this);
                statistics = SqlStatistics.StartTimer(Statistics);
                Task currentReconnectionTask = _currentReconnectionTask;
                if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
                {
                    _reconnectionCancellationSource?.Cancel();
                    AsyncHelper.WaitForCompletion(currentReconnectionTask, 0, null, rethrowExceptions: false);
                    if (State != ConnectionState.Open)
                    {
                        OnStateChange(DbConnectionInternal.StateChangeClosed);
                    }
                }

                CancelOpenAndWait();
                CloseInnerConnection();
                GC.SuppressFinalize(this);
                if (Statistics != null)
                {
                    ADP.TimerCurrent(out _statistics._closeTimestamp);
                }
            }
            catch (OutOfMemoryException e)
            {
                Abort(e);
                throw;
            }
            catch (StackOverflowException e2)
            {
                Abort(e2);
                throw;
            }
            catch (ThreadAbortException e3)
            {
                Abort(e3);
                SqlInternalConnection.BestEffortCleanup(target);
                throw;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
                if (_lastIdentity != null)
                {
                    _lastIdentity.Dispose();
                }
            }
        }
        finally
        {
            SqlDebugContext sdc = _sdc;
            _sdc = null;
            Bid.ScopeLeave(ref hScp);
            sdc?.Dispose();
        }
    }

    //
    // Summary:
    //     Creates and returns a System.Data.SqlClient.SqlCommand object associated with
    //     the System.Data.SqlClient.SqlConnection.
    //
    // Returns:
    //     A System.Data.SqlClient.SqlCommand object.
    public new SqlCommand CreateCommand()
    {
        return new SqlCommand(null, this);
    }

    private void DisposeMe(bool disposing)
    {
        _credential = null;
        _accessToken = null;
        if (!disposing && InnerConnection is SqlInternalConnectionTds sqlInternalConnectionTds && !sqlInternalConnectionTds.ConnectionOptions.Pooling)
        {
            TdsParser parser = sqlInternalConnectionTds.Parser;
            if (parser != null && parser._physicalStateObj != null)
            {
                parser._physicalStateObj.DecrementPendingCallbacks(release: false);
            }
        }
    }

    //
    // Summary:
    //     Enlists in the specified transaction as a distributed transaction.
    //
    // Parameters:
    //   transaction:
    //     A reference to an existing System.EnterpriseServices.ITransaction in which to
    //     enlist.
    public void EnlistDistributedTransaction(ITransaction transaction)
    {
        if (IsContextConnection)
        {
            throw SQL.NotAvailableOnContextConnection();
        }

        EnlistDistributedTransactionHelper(transaction);
    }

    //
    // Summary:
    //     Opens a database connection with the property settings specified by the System.Data.SqlClient.SqlConnection.ConnectionString.
    //
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     Cannot open a connection without specifying a data source or server. or The connection
    //     is already open.
    //
    //   T:System.Data.SqlClient.SqlException:
    //     A connection-level error occurred while opening the connection. If the System.Data.SqlClient.SqlException.Number
    //     property contains the value 18487 or 18488, this indicates that the specified
    //     password has expired or must be reset. See the System.Data.SqlClient.SqlConnection.ChangePassword(System.String,System.String)
    //     method for more information. The <system.data.localdb> tag in the app.config
    //     file has invalid or unknown elements.
    //
    //   T:System.Configuration.ConfigurationErrorsException:
    //     There are two entries with the same name in the <localdbinstances> section.
    public override void Open()
    {
        Bid.ScopeEnter(out var hScp, "<sc.SqlConnection.Open|API> %d#", ObjectID);
        Bid.CorrelationTrace("<sc.SqlConnection.Open|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
        try
        {
            if (StatisticsEnabled)
            {
                if (_statistics == null)
                {
                    _statistics = new SqlStatistics();
                }
                else
                {
                    _statistics.ContinueOnNewConnection();
                }
            }

            SqlStatistics statistics = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                statistics = SqlStatistics.StartTimer(Statistics);
                if (!TryOpen(null))
                {
                    throw ADP.InternalError(ADP.InternalErrorCode.SynchronousConnectReturnedPending);
                }
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
        }
        finally
        {
            Bid.ScopeLeave(ref hScp);
        }
    }

    internal void RegisterWaitingForReconnect(Task waitingTask)
    {
        if (!((SqlConnectionString)ConnectionOptions).MARS)
        {
            Interlocked.CompareExchange(ref _asyncWaitingForReconnection, waitingTask, null);
            if (_asyncWaitingForReconnection != waitingTask)
            {
                throw SQL.MARSUnspportedOnConnection();
            }
        }
    }

    private async Task ReconnectAsync(int timeout)
    {
        _ = 1;
        try
        {
            long commandTimeoutExpiration = 0L;
            if (timeout > 0)
            {
                commandTimeoutExpiration = ADP.TimerCurrent() + ADP.TimerFromSeconds(timeout);
            }

            CancellationToken ctoken = (_reconnectionCancellationSource = new CancellationTokenSource()).Token;
            int retryCount = _connectRetryCount;
            for (int attempt = 0; attempt < retryCount; attempt++)
            {
                if (ctoken.IsCancellationRequested)
                {
                    Bid.Trace("<sc.SqlConnection.ReconnectAsync|INFO> Orginal ClientConnectionID %ls - reconnection cancelled\n", _originalConnectionId.ToString());
                    break;
                }

                try
                {
                    _impersonateIdentity = _lastIdentity;
                    try
                    {
                        base.ForceNewConnection = true;
                        await OpenAsync(ctoken).ConfigureAwait(continueOnCapturedContext: false);
                        _reconnectCount++;
                    }
                    finally
                    {
                        _impersonateIdentity = null;
                        base.ForceNewConnection = false;
                    }

                    Bid.Trace("<sc.SqlConnection.ReconnectIfNeeded|INFO> Reconnection suceeded.  ClientConnectionID %ls -> %ls \n", _originalConnectionId.ToString(), ClientConnectionId.ToString());
                    break;
                }
                catch (SqlException ex)
                {
                    Bid.Trace("<sc.SqlConnection.ReconnectAsyncINFO> Orginal ClientConnectionID %ls - reconnection attempt failed error %ls\n", _originalConnectionId.ToString(), ex.Message);
                    if (attempt == retryCount - 1)
                    {
                        Bid.Trace("<sc.SqlConnection.ReconnectAsync|INFO> Orginal ClientConnectionID %ls - give up reconnection\n", _originalConnectionId.ToString());
                        throw SQL.CR_AllAttemptsFailed(ex, _originalConnectionId);
                    }

                    if (timeout > 0 && ADP.TimerRemaining(commandTimeoutExpiration) < ADP.TimerFromSeconds(ConnectRetryInterval))
                    {
                        throw SQL.CR_NextAttemptWillExceedQueryTimeout(ex, _originalConnectionId);
                    }
                }

                await Task.Delay(1000 * ConnectRetryInterval, ctoken).ConfigureAwait(continueOnCapturedContext: false);
            }
        }
        finally
        {
            _recoverySessionData = null;
            _supressStateChangeForReconnection = false;
        }
    }

    internal Task ValidateAndReconnect(Action beforeDisconnect, int timeout)
    {
        Task task = _currentReconnectionTask;
        while (task != null && task.IsCompleted)
        {
            Interlocked.CompareExchange(ref _currentReconnectionTask, null, task);
            task = _currentReconnectionTask;
        }

        if (task == null)
        {
            if (_connectRetryCount > 0)
            {
                SqlInternalConnectionTds openTdsConnection = GetOpenTdsConnection();
                if (openTdsConnection._sessionRecoveryAcknowledged)
                {
                    TdsParserStateObject physicalStateObj = openTdsConnection.Parser._physicalStateObj;
                    if (!physicalStateObj.ValidateSNIConnection())
                    {
                        if (openTdsConnection.Parser._sessionPool != null && openTdsConnection.Parser._sessionPool.ActiveSessionsCount > 0)
                        {
                            beforeDisconnect?.Invoke();
                            OnError(SQL.CR_UnrecoverableClient(ClientConnectionId), breakConnection: true, null);
                        }

                        SessionData currentSessionData = openTdsConnection.CurrentSessionData;
                        if (currentSessionData._unrecoverableStatesCount == 0)
                        {
                            bool flag = false;
                            lock (_reconnectLock)
                            {
                                openTdsConnection.CheckEnlistedTransactionBinding();
                                task = _currentReconnectionTask;
                                if (task == null)
                                {
                                    if (currentSessionData._unrecoverableStatesCount == 0)
                                    {
                                        _originalConnectionId = ClientConnectionId;
                                        Bid.Trace("<sc.SqlConnection.ReconnectIfNeeded|INFO> Connection ClientConnectionID %ls is invalid, reconnecting\n", _originalConnectionId.ToString());
                                        _recoverySessionData = currentSessionData;
                                        beforeDisconnect?.Invoke();
                                        try
                                        {
                                            _supressStateChangeForReconnection = true;
                                            openTdsConnection.DoomThisConnection();
                                        }
                                        catch (SqlException)
                                        {
                                        }

                                        task = (_currentReconnectionTask = Task.Run(() => ReconnectAsync(timeout)));
                                    }
                                }
                                else
                                {
                                    flag = true;
                                }
                            }

                            if (flag)
                            {
                                beforeDisconnect?.Invoke();
                            }
                        }
                        else
                        {
                            beforeDisconnect?.Invoke();
                            OnError(SQL.CR_UnrecoverableServer(ClientConnectionId), breakConnection: true, null);
                        }
                    }
                }
            }
        }
        else
        {
            beforeDisconnect?.Invoke();
        }

        return task;
    }

    private void WaitForPendingReconnection()
    {
        Task currentReconnectionTask = _currentReconnectionTask;
        if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
        {
            AsyncHelper.WaitForCompletion(currentReconnectionTask, 0, null, rethrowExceptions: false);
        }
    }

    private void CancelOpenAndWait()
    {
        Tuple<TaskCompletionSource<DbConnectionInternal>, Task> currentCompletion = _currentCompletion;
        if (currentCompletion != null)
        {
            currentCompletion.Item1.TrySetCanceled();
            ((IAsyncResult)currentCompletion.Item2).AsyncWaitHandle.WaitOne();
        }
    }

    //
    // Summary:
    //     An asynchronous version of System.Data.SqlClient.SqlConnection.Open, which opens
    //     a database connection with the property settings specified by the System.Data.SqlClient.SqlConnection.ConnectionString.
    //     The cancellation token can be used to request that the operation be abandoned
    //     before the connection timeout elapses. Exceptions will be propagated via the
    //     returned Task. If the connection timeout time elapses without successfully connecting,
    //     the returned Task will be marked as faulted with an Exception. The implementation
    //     returns a Task without blocking the calling thread for both pooled and non-pooled
    //     connections.
    //
    // Parameters:
    //   cancellationToken:
    //     The cancellation instruction.
    //
    // Returns:
    //     A task representing the asynchronous operation.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     Calling System.Data.SqlClient.SqlConnection.OpenAsync(System.Threading.CancellationToken)
    //     more than once for the same instance before task completion. Context Connection=true
    //     is specified in the connection string. A connection was not available from the
    //     connection pool before the connection time out elapsed.
    //
    //   T:System.Data.SqlClient.SqlException:
    //     Any error returned by SQL Server that occurred while opening the connection.
    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        Bid.ScopeEnter(out var hScp, "<sc.SqlConnection.OpenAsync|API> %d#", ObjectID);
        Bid.CorrelationTrace("<sc.SqlConnection.OpenAsync|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
        try
        {
            if (StatisticsEnabled)
            {
                if (_statistics == null)
                {
                    _statistics = new SqlStatistics();
                }
                else
                {
                    _statistics.ContinueOnNewConnection();
                }
            }

            SqlStatistics statistics = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                statistics = SqlStatistics.StartTimer(Statistics);
                Transaction currentTransaction = ADP.GetCurrentTransaction();
                TaskCompletionSource<DbConnectionInternal> completion = new TaskCompletionSource<DbConnectionInternal>(currentTransaction);
                TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>();
                if (cancellationToken.IsCancellationRequested)
                {
                    taskCompletionSource.SetCanceled();
                    return taskCompletionSource.Task;
                }

                if (IsContextConnection)
                {
                    taskCompletionSource.SetException(ADP.ExceptionWithStackTrace(SQL.NotAvailableOnContextConnection()));
                    return taskCompletionSource.Task;
                }

                bool flag;
                try
                {
                    flag = TryOpen(completion);
                }
                catch (Exception exception)
                {
                    taskCompletionSource.SetException(exception);
                    return taskCompletionSource.Task;
                }

                if (flag)
                {
                    taskCompletionSource.SetResult(null);
                    return taskCompletionSource.Task;
                }

                CancellationTokenRegistration registration = default(CancellationTokenRegistration);
                if (cancellationToken.CanBeCanceled)
                {
                    registration = cancellationToken.Register(delegate
                    {
                        completion.TrySetCanceled();
                    });
                }

                OpenAsyncRetry @object = new OpenAsyncRetry(this, completion, taskCompletionSource, registration);
                _currentCompletion = new Tuple<TaskCompletionSource<DbConnectionInternal>, Task>(completion, taskCompletionSource.Task);
                completion.Task.ContinueWith(@object.Retry, TaskScheduler.Default);
                return taskCompletionSource.Task;
            }
            finally
            {
                SqlStatistics.StopTimer(statistics);
            }
        }
        finally
        {
            Bid.ScopeLeave(ref hScp);
        }
    }

    private bool TryOpen(TaskCompletionSource<DbConnectionInternal> retry)
    {
        SqlConnectionString sqlConnectionString = (SqlConnectionString)ConnectionOptions;
        _applyTransientFaultHandling = retry == null && sqlConnectionString != null && sqlConnectionString.ConnectRetryCount > 0;
        if (sqlConnectionString != null && (sqlConnectionString.Authentication == SqlAuthenticationMethod.SqlPassword || sqlConnectionString.Authentication == SqlAuthenticationMethod.ActiveDirectoryPassword) && (!sqlConnectionString.HasUserIdKeyword || !sqlConnectionString.HasPasswordKeyword) && _credential == null)
        {
            throw SQL.CredentialsNotProvided(sqlConnectionString.Authentication);
        }

        if (_impersonateIdentity != null)
        {
            using (WindowsIdentity windowsIdentity = DbConnectionPoolIdentity.GetCurrentWindowsIdentity())
            {
                if (!(_impersonateIdentity.User == windowsIdentity.User))
                {
                    using (_impersonateIdentity.Impersonate())
                    {
                        return TryOpenInner(retry);
                    }
                }

                return TryOpenInner(retry);
            }
        }

        if (UsesIntegratedSecurity(sqlConnectionString) || UsesActiveDirectoryIntegrated(sqlConnectionString))
        {
            _lastIdentity = DbConnectionPoolIdentity.GetCurrentWindowsIdentity();
        }
        else
        {
            _lastIdentity = null;
        }

        return TryOpenInner(retry);
    }

    private bool TryOpenInner(TaskCompletionSource<DbConnectionInternal> retry)
    {
        TdsParser target = null;
        RuntimeHelpers.PrepareConstrainedRegions();
        try
        {
            if (base.ForceNewConnection)
            {
                if (!InnerConnection.TryReplaceConnection(this, ConnectionFactory, retry, UserConnectionOptions))
                {
                    return false;
                }
            }
            else if (!InnerConnection.TryOpenConnection(this, ConnectionFactory, retry, UserConnectionOptions))
            {
                return false;
            }

            target = SqlInternalConnection.GetBestEffortCleanupTarget(this);
            if (!(InnerConnection is SqlInternalConnectionTds sqlInternalConnectionTds))
            {
                SqlInternalConnectionSmi sqlInternalConnectionSmi = InnerConnection as SqlInternalConnectionSmi;
                sqlInternalConnectionSmi.AutomaticEnlistment();
            }
            else
            {
                if (!sqlInternalConnectionTds.ConnectionOptions.Pooling)
                {
                    GC.ReRegisterForFinalize(this);
                }

                if (StatisticsEnabled)
                {
                    ADP.TimerCurrent(out _statistics._openTimestamp);
                    sqlInternalConnectionTds.Parser.Statistics = _statistics;
                }
                else
                {
                    sqlInternalConnectionTds.Parser.Statistics = null;
                    _statistics = null;
                }

                CompleteOpen();
            }
        }
        catch (OutOfMemoryException e)
        {
            Abort(e);
            throw;
        }
        catch (StackOverflowException e2)
        {
            Abort(e2);
            throw;
        }
        catch (ThreadAbortException e3)
        {
            Abort(e3);
            SqlInternalConnection.BestEffortCleanup(target);
            throw;
        }

        return true;
    }

    internal void ValidateConnectionForExecute(string method, SqlCommand command)
    {
        Task asyncWaitingForReconnection = _asyncWaitingForReconnection;
        if (asyncWaitingForReconnection != null)
        {
            if (!asyncWaitingForReconnection.IsCompleted)
            {
                throw SQL.MARSUnspportedOnConnection();
            }

            Interlocked.CompareExchange(ref _asyncWaitingForReconnection, null, asyncWaitingForReconnection);
        }

        if (_currentReconnectionTask != null)
        {
            Task currentReconnectionTask = _currentReconnectionTask;
            if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted)
            {
                return;
            }
        }

        SqlInternalConnection openConnection = GetOpenConnection(method);
        openConnection.ValidateConnectionForExecute(command);
    }

    internal static string FixupDatabaseTransactionName(string name)
    {
        if (!ADP.IsEmpty(name))
        {
            return SqlServerEscapeHelper.EscapeIdentifier(name);
        }

        return name;
    }

    internal void OnError(SqlException exception, bool breakConnection, Action<Action> wrapCloseInAction)
    {
        if (breakConnection && ConnectionState.Open == State)
        {
            if (wrapCloseInAction != null)
            {
                int capturedCloseCount = _closeCount;
                Action obj = delegate
                {
                    if (capturedCloseCount == _closeCount)
                    {
                        Bid.Trace("<sc.SqlConnection.OnError|INFO> %d#, Connection broken.\n", ObjectID);
                        Close();
                    }
                };
                wrapCloseInAction(obj);
            }
            else
            {
                Bid.Trace("<sc.SqlConnection.OnError|INFO> %d#, Connection broken.\n", ObjectID);
                Close();
            }
        }

        if (exception.Class >= 11)
        {
            throw exception;
        }

        OnInfoMessage(new SqlInfoMessageEventArgs(exception));
    }

    private void CompleteOpen()
    {
        if (!GetOpenConnection().IsYukonOrNewer && Debugger.IsAttached)
        {
            bool flag = false;
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                flag = true;
            }
            catch (SecurityException e)
            {
                ADP.TraceExceptionWithoutRethrow(e);
            }

            if (flag)
            {
                CheckSQLDebugOnConnect();
            }
        }
    }

    internal SqlInternalConnection GetOpenConnection()
    {
        if (!(InnerConnection is SqlInternalConnection result))
        {
            throw ADP.ClosedConnectionError();
        }

        return result;
    }

    internal SqlInternalConnection GetOpenConnection(string method)
    {
        DbConnectionInternal innerConnection = InnerConnection;
        if (!(innerConnection is SqlInternalConnection result))
        {
            throw ADP.OpenConnectionRequired(method, innerConnection.State);
        }

        return result;
    }

    internal SqlInternalConnectionTds GetOpenTdsConnection()
    {
        if (!(InnerConnection is SqlInternalConnectionTds result))
        {
            throw ADP.ClosedConnectionError();
        }

        return result;
    }

    internal SqlInternalConnectionTds GetOpenTdsConnection(string method)
    {
        if (!(InnerConnection is SqlInternalConnectionTds result))
        {
            throw ADP.OpenConnectionRequired(method, InnerConnection.State);
        }

        return result;
    }

    internal void OnInfoMessage(SqlInfoMessageEventArgs imevent)
    {
        OnInfoMessage(imevent, out var _);
    }

    internal void OnInfoMessage(SqlInfoMessageEventArgs imevent, out bool notified)
    {
        if (Bid.TraceOn)
        {
            Bid.Trace("<sc.SqlConnection.OnInfoMessage|API|INFO> %d#, Message='%ls'\n", ObjectID, (imevent != null) ? imevent.Message : "");
        }

        SqlInfoMessageEventHandler sqlInfoMessageEventHandler = (SqlInfoMessageEventHandler)base.Events[EventInfoMessage];
        if (sqlInfoMessageEventHandler != null)
        {
            notified = true;
            try
            {
                sqlInfoMessageEventHandler(this, imevent);
                return;
            }
            catch (Exception e)
            {
                if (!ADP.IsCatchableOrSecurityExceptionType(e))
                {
                    throw;
                }

                ADP.TraceExceptionWithoutRethrow(e);
                return;
            }
        }

        notified = false;
    }

    private void CheckSQLDebugOnConnect()
    {
        uint currentProcessId = (uint)SafeNativeMethods.GetCurrentProcessId();
        string text = ((!ADP.IsPlatformNT5) ? "SqlClientSSDebug" : "Global\\SqlClientSSDebug");
        text += currentProcessId.ToString(CultureInfo.InvariantCulture);
        IntPtr intPtr = NativeMethods.OpenFileMappingA(4, bInheritHandle: false, text);
        if (ADP.PtrZero != intPtr)
        {
            IntPtr intPtr2 = NativeMethods.MapViewOfFile(intPtr, 4, 0, 0, IntPtr.Zero);
            if (ADP.PtrZero != intPtr2)
            {
                SqlDebugContext sqlDebugContext = new SqlDebugContext();
                sqlDebugContext.hMemMap = intPtr;
                sqlDebugContext.pMemMap = intPtr2;
                sqlDebugContext.pid = currentProcessId;
                CheckSQLDebug(sqlDebugContext);
                _sdc = sqlDebugContext;
            }
        }
    }

    internal void CheckSQLDebug()
    {
        if (_sdc != null)
        {
            CheckSQLDebug(_sdc);
        }
    }

    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    private void CheckSQLDebug(SqlDebugContext sdc)
    {
        uint currentThreadId = (uint)AppDomain.GetCurrentThreadId();
        RefreshMemoryMappedData(sdc);
        if (!sdc.active && sdc.fOption)
        {
            sdc.active = true;
            sdc.tid = currentThreadId;
            try
            {
                IssueSQLDebug(1u, sdc.machineName, sdc.pid, sdc.dbgpid, sdc.sdiDllName, sdc.data);
                sdc.tid = 0u;
            }
            catch
            {
                sdc.active = false;
                throw;
            }
        }

        if (!sdc.active)
        {
            return;
        }

        if (!sdc.fOption)
        {
            sdc.Dispose();
            IssueSQLDebug(0u, null, 0u, 0u, null, null);
        }
        else if (sdc.tid != currentThreadId)
        {
            sdc.tid = currentThreadId;
            try
            {
                IssueSQLDebug(2u, null, sdc.pid, sdc.tid, null, null);
            }
            catch
            {
                sdc.tid = 0u;
                throw;
            }
        }
    }

    private void IssueSQLDebug(uint option, string machineName, uint pid, uint id, string sdiDllName, byte[] data)
    {
        if (!GetOpenConnection().IsYukonOrNewer)
        {
            SqlCommand sqlCommand = new SqlCommand("sp_sdidebug", this);
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlParameter sqlParameter = new SqlParameter(null, SqlDbType.VarChar, TdsEnums.SQLDEBUG_MODE_NAMES[option].Length);
            sqlParameter.Value = TdsEnums.SQLDEBUG_MODE_NAMES[option];
            sqlCommand.Parameters.Add(sqlParameter);
            if (option == 1)
            {
                sqlParameter = new SqlParameter(null, SqlDbType.VarChar, sdiDllName.Length);
                sqlParameter.Value = sdiDllName;
                sqlCommand.Parameters.Add(sqlParameter);
                sqlParameter = new SqlParameter(null, SqlDbType.VarChar, machineName.Length);
                sqlParameter.Value = machineName;
                sqlCommand.Parameters.Add(sqlParameter);
            }

            if (option != 0)
            {
                sqlParameter = new SqlParameter(null, SqlDbType.Int);
                sqlParameter.Value = pid;
                sqlCommand.Parameters.Add(sqlParameter);
                sqlParameter = new SqlParameter(null, SqlDbType.Int);
                sqlParameter.Value = id;
                sqlCommand.Parameters.Add(sqlParameter);
            }

            if (option == 1)
            {
                sqlParameter = new SqlParameter(null, SqlDbType.VarBinary, (data != null) ? data.Length : 0);
                sqlParameter.Value = data;
                sqlCommand.Parameters.Add(sqlParameter);
            }

            sqlCommand.ExecuteNonQuery();
        }
    }

    //
    // Summary:
    //     Changes the SQL Server password for the user indicated in the connection string
    //     to the supplied new password.
    //
    // Parameters:
    //   connectionString:
    //     The connection string that contains enough information to connect to the server
    //     that you want. The connection string must contain the user ID and the current
    //     password.
    //
    //   newPassword:
    //     The new password to set. This password must comply with any password security
    //     policy set on the server, including minimum length, requirements for specific
    //     characters, and so on.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     The connection string includes the option to use integrated security. Or The
    //     newPassword exceeds 128 characters.
    //
    //   T:System.ArgumentNullException:
    //     Either the connectionString or the newPassword parameter is null.
    public static void ChangePassword(string connectionString, string newPassword)
    {
        Bid.ScopeEnter(out var hScp, "<sc.SqlConnection.ChangePassword|API>");
        Bid.CorrelationTrace("<sc.SqlConnection.ChangePassword|API|Correlation> ActivityID %ls\n");
        try
        {
            if (ADP.IsEmpty(connectionString))
            {
                throw SQL.ChangePasswordArgumentMissing("connectionString");
            }

            if (ADP.IsEmpty(newPassword))
            {
                throw SQL.ChangePasswordArgumentMissing("newPassword");
            }

            if (128 < newPassword.Length)
            {
                throw ADP.InvalidArgumentLength("newPassword", 128);
            }

            SqlConnectionPoolKey key = new SqlConnectionPoolKey(connectionString, null, null);
            SqlConnectionString sqlConnectionString = SqlConnectionFactory.FindSqlConnectionOptions(key);
            if (sqlConnectionString.IntegratedSecurity || sqlConnectionString.Authentication == SqlAuthenticationMethod.ActiveDirectoryIntegrated)
            {
                throw SQL.ChangePasswordConflictsWithSSPI();
            }

            if (!ADP.IsEmpty(sqlConnectionString.AttachDBFilename))
            {
                throw SQL.ChangePasswordUseOfUnallowedKey("attachdbfilename");
            }

            if (sqlConnectionString.ContextConnection)
            {
                throw SQL.ChangePasswordUseOfUnallowedKey("context connection");
            }

            PermissionSet permissionSet = sqlConnectionString.CreatePermissionSet();
            permissionSet.Demand();
            ChangePassword(connectionString, sqlConnectionString, null, newPassword, null);
        }
        finally
        {
            Bid.ScopeLeave(ref hScp);
        }
    }

    //
    // Summary:
    //     Changes the SQL Server password for the user indicated in the System.Data.SqlClient.SqlCredential
    //     object.
    //
    // Parameters:
    //   connectionString:
    //     The connection string that contains enough information to connect to a server.
    //     The connection string should not use any of the following connection string keywords:
    //     Integrated Security = true, UserId, or Password; or ContextConnection = true.
    //
    //
    //   credential:
    //     A System.Data.SqlClient.SqlCredential object.
    //
    //   newSecurePassword:
    //     The new password. newSecurePassword must be read only. The password must also
    //     comply with any password security policy set on the server (for example, minimum
    //     length and requirements for specific characters).
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     The connection string contains any combination of UserId, Password, or Integrated
    //     Security=true. -or- The connection string contains Context Connection=true. -or-
    //     newSecurePassword (or newPassword) is greater than 128 characters. -or- newSecurePassword
    //     (or newPassword) is not read only. -or- newSecurePassword (or newPassword) is
    //     an empty string.
    //
    //   T:System.ArgumentNullException:
    //     One of the parameters (connectionString, credential, or newSecurePassword) is
    //     null.
    public static void ChangePassword(string connectionString, SqlCredential credential, SecureString newSecurePassword)
    {
        Bid.ScopeEnter(out var hScp, "<sc.SqlConnection.ChangePassword|API>");
        Bid.CorrelationTrace("<sc.SqlConnection.ChangePassword|API|Correlation> ActivityID %ls\n");
        try
        {
            if (ADP.IsEmpty(connectionString))
            {
                throw SQL.ChangePasswordArgumentMissing("connectionString");
            }

            if (credential == null)
            {
                throw SQL.ChangePasswordArgumentMissing("credential");
            }

            if (newSecurePassword == null || newSecurePassword.Length == 0)
            {
                throw SQL.ChangePasswordArgumentMissing("newSecurePassword");
            }

            if (!newSecurePassword.IsReadOnly())
            {
                throw ADP.MustBeReadOnly("newSecurePassword");
            }

            if (128 < newSecurePassword.Length)
            {
                throw ADP.InvalidArgumentLength("newSecurePassword", 128);
            }

            SqlConnectionPoolKey key = new SqlConnectionPoolKey(connectionString, credential, null);
            SqlConnectionString sqlConnectionString = SqlConnectionFactory.FindSqlConnectionOptions(key);
            if (!ADP.IsEmpty(sqlConnectionString.UserID) || !ADP.IsEmpty(sqlConnectionString.Password))
            {
                throw ADP.InvalidMixedArgumentOfSecureAndClearCredential();
            }

            if (sqlConnectionString.IntegratedSecurity || sqlConnectionString.Authentication == SqlAuthenticationMethod.ActiveDirectoryIntegrated)
            {
                throw SQL.ChangePasswordConflictsWithSSPI();
            }

            if (!ADP.IsEmpty(sqlConnectionString.AttachDBFilename))
            {
                throw SQL.ChangePasswordUseOfUnallowedKey("attachdbfilename");
            }

            if (sqlConnectionString.ContextConnection)
            {
                throw SQL.ChangePasswordUseOfUnallowedKey("context connection");
            }

            PermissionSet permissionSet = sqlConnectionString.CreatePermissionSet();
            permissionSet.Demand();
            ChangePassword(connectionString, sqlConnectionString, credential, null, newSecurePassword);
        }
        finally
        {
            Bid.ScopeLeave(ref hScp);
        }
    }

    private static void ChangePassword(string connectionString, SqlConnectionString connectionOptions, SqlCredential credential, string newPassword, SecureString newSecurePassword)
    {
        using (SqlInternalConnectionTds sqlInternalConnectionTds = new SqlInternalConnectionTds(null, connectionOptions, credential, null, newPassword, newSecurePassword, redirectedUserInstance: false))
        {
            if (!sqlInternalConnectionTds.IsYukonOrNewer)
            {
                throw SQL.ChangePasswordRequiresYukon();
            }
        }

        SqlConnectionPoolKey key = new SqlConnectionPoolKey(connectionString, credential, null);
        SqlConnectionFactory.SingletonInstance.ClearPool(key);
    }

    internal void RegisterForConnectionCloseNotification<T>(ref Task<T> outterTask, object value, int tag)
    {
        outterTask = outterTask.ContinueWith(delegate (Task<T> task)
        {
            RemoveWeakReference(value);
            return task;
        }, TaskScheduler.Default).Unwrap();
    }

    private static void RefreshMemoryMappedData(SqlDebugContext sdc)
    {
        MEMMAP mEMMAP = (MEMMAP)Marshal.PtrToStructure(sdc.pMemMap, typeof(MEMMAP));
        sdc.dbgpid = mEMMAP.dbgpid;
        sdc.fOption = mEMMAP.fOption == 1;
        Encoding encoding = Encoding.GetEncoding(1252);
        sdc.machineName = encoding.GetString(mEMMAP.rgbMachineName, 0, mEMMAP.rgbMachineName.Length);
        sdc.sdiDllName = encoding.GetString(mEMMAP.rgbDllName, 0, mEMMAP.rgbDllName.Length);
        sdc.data = mEMMAP.rgbData;
    }

    //
    // Summary:
    //     If statistics gathering is enabled, all values are reset to zero.
    public void ResetStatistics()
    {
        if (IsContextConnection)
        {
            throw SQL.NotAvailableOnContextConnection();
        }

        if (Statistics != null)
        {
            Statistics.Reset();
            if (ConnectionState.Open == State)
            {
                ADP.TimerCurrent(out _statistics._openTimestamp);
            }
        }
    }

    //
    // Summary:
    //     Returns a name value pair collection of statistics at the point in time the method
    //     is called.
    //
    // Returns:
    //     Returns a reference of type System.Collections.IDictionary of System.Collections.DictionaryEntry
    //     items.
    public IDictionary RetrieveStatistics()
    {
        if (IsContextConnection)
        {
            throw SQL.NotAvailableOnContextConnection();
        }

        if (Statistics != null)
        {
            UpdateStatistics();
            return Statistics.GetHashtable();
        }

        return new SqlStatistics().GetHashtable();
    }

    private void UpdateStatistics()
    {
        if (ConnectionState.Open == State)
        {
            ADP.TimerCurrent(out _statistics._closeTimestamp);
        }

        Statistics.UpdateStatistics();
    }

    private Assembly ResolveTypeAssembly(AssemblyName asmRef, bool throwOnError)
    {
        if (string.Compare(asmRef.Name, "Microsoft.SqlServer.Types", StringComparison.OrdinalIgnoreCase) == 0)
        {
            if (Bid.TraceOn && asmRef.Version != TypeSystemAssemblyVersion)
            {
                Bid.Trace("<sc.SqlConnection.ResolveTypeAssembly> SQL CLR type version change: Server sent %ls, client will instantiate %ls", asmRef.Version.ToString(), TypeSystemAssemblyVersion.ToString());
            }

            asmRef.Version = TypeSystemAssemblyVersion;
        }

        try
        {
            return Assembly.Load(asmRef);
        }
        catch (Exception e)
        {
            if (throwOnError || !ADP.IsCatchableExceptionType(e))
            {
                throw;
            }

            return null;
        }
    }

    internal void CheckGetExtendedUDTInfo(SqlMetaDataPriv metaData, bool fThrow)
    {
        if (metaData.udtType == null)
        {
            metaData.udtType = Type.GetType(metaData.udtAssemblyQualifiedName, (AssemblyName asmRef) => ResolveTypeAssembly(asmRef, fThrow), null, fThrow);
            if (fThrow && metaData.udtType == null)
            {
                throw SQL.UDTUnexpectedResult(metaData.udtAssemblyQualifiedName);
            }
        }
    }

    internal object GetUdtValue(object value, SqlMetaDataPriv metaData, bool returnDBNull)
    {
        if (returnDBNull && ADP.IsNull(value))
        {
            return DBNull.Value;
        }

        object obj = null;
        if (ADP.IsNull(value))
        {
            Type udtType = metaData.udtType;
            return udtType.InvokeMember("Null", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty, null, null, new object[0], CultureInfo.InvariantCulture);
        }

        MemoryStream s = new MemoryStream((byte[])value);
        return SerializationHelperSql9.Deserialize(s, metaData.udtType);
    }

    internal byte[] GetBytes(object o)
    {
        Format format = Format.Native;
        int maxSize = 0;
        return GetBytes(o, out format, out maxSize);
    }

    internal byte[] GetBytes(object o, out Format format, out int maxSize)
    {
        SqlUdtInfo infoFromType = AssemblyCache.GetInfoFromType(o.GetType());
        maxSize = infoFromType.MaxByteSize;
        format = infoFromType.SerializationFormat;
        if (maxSize < -1 || maxSize >= 65535)
        {
            throw new InvalidOperationException(o.GetType()?.ToString() + ": invalid Size");
        }

        using MemoryStream memoryStream = new MemoryStream((maxSize >= 0) ? maxSize : 0);
        SerializationHelperSql9.Serialize(memoryStream, o);
        return memoryStream.ToArray();
    }

    //
    // Summary:
    //     Initializes a new instance of the System.Data.SqlClient.SqlConnection class.
    public SqlConnection()
    {
        GC.SuppressFinalize(this);
        _innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
    }

    private void CopyFrom(SqlConnection connection)
    {
        ADP.CheckArgumentNull(connection, "connection");
        _userConnectionOptions = connection.UserConnectionOptions;
        _poolGroup = connection.PoolGroup;
        if (DbConnectionClosedNeverOpened.SingletonInstance == connection._innerConnection)
        {
            _innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
        }
        else
        {
            _innerConnection = DbConnectionClosedPreviouslyOpened.SingletonInstance;
        }
    }

    private string ConnectionString_Get()
    {
        Bid.Trace("<prov.DbConnectionHelper.ConnectionString_Get|API> %d#\n", ObjectID);
        bool shouldHidePassword = InnerConnection.ShouldHidePassword;
        DbConnectionOptions userConnectionOptions = UserConnectionOptions;
        if (userConnectionOptions == null)
        {
            return "";
        }

        return userConnectionOptions.UsersConnectionString(shouldHidePassword);
    }

    private void ConnectionString_Set(string value)
    {
        DbConnectionPoolKey key = new DbConnectionPoolKey(value);
        ConnectionString_Set(key);
    }

    private void ConnectionString_Set(DbConnectionPoolKey key)
    {
        DbConnectionOptions userConnectionOptions = null;
        DbConnectionPoolGroup connectionPoolGroup = ConnectionFactory.GetConnectionPoolGroup(key, null, ref userConnectionOptions);
        DbConnectionInternal innerConnection = InnerConnection;
        bool flag = innerConnection.AllowSetConnectionString;
        if (flag)
        {
            flag = SetInnerConnectionFrom(DbConnectionClosedBusy.SingletonInstance, innerConnection);
            if (flag)
            {
                _userConnectionOptions = userConnectionOptions;
                _poolGroup = connectionPoolGroup;
                _innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
            }
        }

        if (!flag)
        {
            throw ADP.OpenConnectionPropertySet("ConnectionString", innerConnection.State);
        }

        if (Bid.TraceOn)
        {
            string a = ((userConnectionOptions != null) ? userConnectionOptions.UsersConnectionStringForTrace() : "");
            Bid.Trace("<prov.DbConnectionHelper.ConnectionString_Set|API> %d#, '%ls'\n", ObjectID, a);
        }
    }

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    internal void Abort(Exception e)
    {
        DbConnectionInternal innerConnection = _innerConnection;
        if (ConnectionState.Open == innerConnection.State)
        {
            Interlocked.CompareExchange(ref _innerConnection, DbConnectionClosedPreviouslyOpened.SingletonInstance, innerConnection);
            innerConnection.DoomThisConnection();
        }

        if (e is OutOfMemoryException)
        {
            Bid.Trace("<prov.DbConnectionHelper.Abort|RES|INFO|CPOOL> %d#, Aborting operation due to asynchronous exception: %ls\n", ObjectID, "OutOfMemory");
        }
        else
        {
            Bid.Trace("<prov.DbConnectionHelper.Abort|RES|INFO|CPOOL> %d#, Aborting operation due to asynchronous exception: %ls\n", ObjectID, e.ToString());
        }
    }

    internal void AddWeakReference(object value, int tag)
    {
        InnerConnection.AddWeakReference(value, tag);
    }

    protected override DbCommand CreateDbCommand()
    {
        DbCommand dbCommand = null;
        Bid.ScopeEnter(out var hScp, "<prov.DbConnectionHelper.CreateDbCommand|API> %d#\n", ObjectID);
        try
        {
            DbProviderFactory providerFactory = ConnectionFactory.ProviderFactory;
            dbCommand = providerFactory.CreateCommand();
            dbCommand.Connection = this;
            return dbCommand;
        }
        finally
        {
            Bid.ScopeLeave(ref hScp);
        }
    }

    private static CodeAccessPermission CreateExecutePermission()
    {
        DBDataPermission dBDataPermission = (DBDataPermission)SqlConnectionFactory.SingletonInstance.ProviderFactory.CreatePermission(PermissionState.None);
        dBDataPermission.Add(string.Empty, string.Empty, KeyRestrictionBehavior.AllowOnly);
        return dBDataPermission;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _userConnectionOptions = null;
            _poolGroup = null;
            Close();
        }

        DisposeMe(disposing);
        base.Dispose(disposing);
    }

    private void RepairInnerConnection()
    {
        WaitForPendingReconnection();
        if (_connectRetryCount != 0 && InnerConnection is SqlInternalConnectionTds sqlInternalConnectionTds)
        {
            sqlInternalConnectionTds.ValidateConnectionForExecute(null);
            sqlInternalConnectionTds.GetSessionAndReconnectIfNeeded(this);
        }
    }

    private void EnlistDistributedTransactionHelper(ITransaction transaction)
    {
        //IL_003c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0046: Expected O, but got Unknown
        PermissionSet permissionSet = new PermissionSet(PermissionState.None);
        permissionSet.AddPermission(ExecutePermission);
        permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
        permissionSet.Demand();
        Bid.Trace("<prov.DbConnectionHelper.EnlistDistributedTransactionHelper|RES|TRAN> %d#, Connection enlisting in a transaction.\n", ObjectID);
        Transaction transaction2 = null;
        if (transaction != null)
        {
            transaction2 = TransactionInterop.GetTransactionFromDtcTransaction((IDtcTransaction)transaction);
        }

        RepairInnerConnection();
        InnerConnection.EnlistTransaction(transaction2);
        GC.KeepAlive(this);
    }

    //
    // Summary:
    //     Enlists in the specified transaction as a distributed transaction.
    //
    // Parameters:
    //   transaction:
    //     A reference to an existing System.Transactions.Transaction in which to enlist.
    public override void EnlistTransaction(Transaction transaction)
    {
        //IL_0041: Unknown result type (might be due to invalid IL or missing references)
        ExecutePermission.Demand();
        Bid.Trace("<prov.DbConnectionHelper.EnlistTransaction|RES|TRAN> %d#, Connection enlisting in a transaction.\n", ObjectID);
        DbConnectionInternal innerConnection = InnerConnection;
        Transaction enlistedTransaction = innerConnection.EnlistedTransaction;
        if (enlistedTransaction != (Transaction)null)
        {
            if (((object)enlistedTransaction).Equals((object)transaction))
            {
                return;
            }

            if ((int)enlistedTransaction.TransactionInformation.Status == 0)
            {
                throw ADP.TransactionPresent();
            }
        }

        RepairInnerConnection();
        InnerConnection.EnlistTransaction(transaction);
        GC.KeepAlive(this);
    }

    private DbMetaDataFactory GetMetaDataFactory(DbConnectionInternal internalConnection)
    {
        return ConnectionFactory.GetMetaDataFactory(_poolGroup, internalConnection);
    }

    internal DbMetaDataFactory GetMetaDataFactoryInternal(DbConnectionInternal internalConnection)
    {
        return GetMetaDataFactory(internalConnection);
    }

    //
    // Summary:
    //     Returns schema information for the data source of this System.Data.SqlClient.SqlConnection.
    //     For more information about scheme, see SQL Server Schema Collections.
    //
    // Returns:
    //     A System.Data.DataTable that contains schema information.
    public override DataTable GetSchema()
    {
        return GetSchema(DbMetaDataCollectionNames.MetaDataCollections, null);
    }

    //
    // Summary:
    //     Returns schema information for the data source of this System.Data.SqlClient.SqlConnection
    //     using the specified string for the schema name.
    //
    // Parameters:
    //   collectionName:
    //     Specifies the name of the schema to return.
    //
    // Returns:
    //     A System.Data.DataTable that contains schema information.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     collectionName is specified as null.
    public override DataTable GetSchema(string collectionName)
    {
        return GetSchema(collectionName, null);
    }

    //
    // Summary:
    //     Returns schema information for the data source of this System.Data.SqlClient.SqlConnection
    //     using the specified string for the schema name and the specified string array
    //     for the restriction values.
    //
    // Parameters:
    //   collectionName:
    //     Specifies the name of the schema to return.
    //
    //   restrictionValues:
    //     A set of restriction values for the requested schema.
    //
    // Returns:
    //     A System.Data.DataTable that contains schema information.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     collectionName is specified as null.
    public override DataTable GetSchema(string collectionName, string[] restrictionValues)
    {
        ExecutePermission.Demand();
        return InnerConnection.GetSchema(ConnectionFactory, PoolGroup, this, collectionName, restrictionValues);
    }

    internal void NotifyWeakReference(int message)
    {
        InnerConnection.NotifyWeakReference(message);
    }

    internal void PermissionDemand()
    {
        DbConnectionOptions dbConnectionOptions = PoolGroup?.ConnectionOptions;
        if (dbConnectionOptions == null || dbConnectionOptions.IsEmpty)
        {
            throw ADP.NoConnectionString();
        }

        DbConnectionOptions userConnectionOptions = UserConnectionOptions;
        userConnectionOptions.DemandPermission();
    }

    internal void RemoveWeakReference(object value)
    {
        InnerConnection.RemoveWeakReference(value);
    }

    internal void SetInnerConnectionEvent(DbConnectionInternal to)
    {
        ConnectionState connectionState = _innerConnection.State & ConnectionState.Open;
        ConnectionState connectionState2 = to.State & ConnectionState.Open;
        if (connectionState != connectionState2 && connectionState2 == ConnectionState.Closed)
        {
            _closeCount++;
        }

        _innerConnection = to;
        if (connectionState == ConnectionState.Closed && ConnectionState.Open == connectionState2)
        {
            OnStateChange(DbConnectionInternal.StateChangeOpen);
        }
        else if (ConnectionState.Open == connectionState && connectionState2 == ConnectionState.Closed)
        {
            OnStateChange(DbConnectionInternal.StateChangeClosed);
        }
        else if (connectionState != connectionState2)
        {
            OnStateChange(new StateChangeEventArgs(connectionState, connectionState2));
        }
    }

    internal bool SetInnerConnectionFrom(DbConnectionInternal to, DbConnectionInternal from)
    {
        return from == Interlocked.CompareExchange(ref _innerConnection, to, from);
    }

    internal void SetInnerConnectionTo(DbConnectionInternal to)
    {
        _innerConnection = to;
    }

    [Conditional("DEBUG")]
    internal static void VerifyExecutePermission()
    {
        try
        {
            ExecutePermission.Demand();
        }
        catch (SecurityException)
        {
            throw;
        }
    }
}
#if false // Decompilation log
'12' items in cache
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll'
------------------
Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.dll'
------------------
Resolve: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Xml.dll'
------------------
Resolve: 'System.Transactions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Could not find by name: 'System.Transactions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
------------------
Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Core.dll'
------------------
Resolve: 'System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
------------------
Resolve: 'System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Could not find by name: 'System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
------------------
Resolve: 'System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'System.Runtime.Caching, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
------------------
Resolve: 'System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
#endif
