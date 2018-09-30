using System.Collections.Generic;
using Foxpict.Client.Sdk.Infra;
using Foxpict.Client.Sdk.Models;

namespace Foxpict.Client.Web.Dao {
  /// <summary>
  /// ラベル情報のDAOクラス
  /// </summary>
  public class LabelDao : ILabelDao {
    public ICollection<Label> LoadLabel () {
      return new Label[0];
    }

    public Label LoadLabel (long labelId) {
      return new Label { Id = labelId, Name = "DUMMY" };
    }

    public ICollection<Category> LoadLabelLinkCategory (string query, int offset, int limit) {
      return new Category[0];
    }

    public ICollection<Label> LoadRoot () {
      return new Label[0];
    }
  }
}
