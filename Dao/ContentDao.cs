using Foxpict.Client.Sdk.Infra;
using Foxpict.Client.Sdk.Models;

namespace Foxpict.Client.Web.Dao {
  /// <summary>
  /// コンテント情報のDAOクラス
  /// </summary>
  public class ContentDao : IContentDao {
    public Content LoadContent (long contentId) {
      return new Content { Id = contentId, Name = "DUMMY" };
    }

    public void Update (Content content) {
      // EMPTY
    }

    public void UpdateRead (long contentId) {
      // EMPTY
    }
  }
}
