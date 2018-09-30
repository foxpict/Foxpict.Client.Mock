namespace Foxpict.Client.Web.Model {
  public class MockAppSettings {
    /// <summary>
    /// MockDAOを使用するかどうかのフラグです
    /// </summary>
    /// <value></value>
    public bool EnableMockDao { get; set; }

    /// <summary>
    /// サービスサーバのURLです。
    /// サービスサーバを使用する場合は、EnableMockDaoをfalseに設定します。
    /// </summary>
    /// <value></value>
    public string ENV_SERVICESERVER_URL { get;set;}
  }
}
