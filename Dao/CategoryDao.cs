using System;
using System.Collections.Generic;
using Bogus;
using Foxpict.Client.Sdk.Infra;
using Foxpict.Client.Sdk.Models;
using NLog;

namespace Foxpict.Client.Web.Dao {
  public class CategoryDao : ICategoryDao {
    readonly Logger mlogger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public CategoryDao () {
      this.mlogger = LogManager.GetCurrentClassLogger ();
    }

    /// <summary>
    /// カテゴリを読み込みます
    /// </summary>
    /// <param name="categoryId"></param>
    /// <param name="offsetSubCategory"></param>
    /// <param name="limitSubCategory"></param>
    /// <param name="offsetContent"></param>
    /// <returns></returns>
    public Category LoadCategory (long categoryId, int offsetSubCategory = 0, int limitSubCategory = Constant.MAXLIMIT, int offsetContent = 0) {
      this.mlogger.Trace ("EXECUTE");

      Randomizer.Seed = new Random ((int) categoryId);
      var testData = new Faker<Category> ()
        .RuleFor (o => o.Id, f => f.Random.Number (100))
        .RuleFor (o => o.Name, f => f.Name.FirstName ())
        .RuleFor (o => o.HasLinkSubCategoryFlag, f => f.Random.Bool ());
      var testData2 = new Faker<Content> ()
        .RuleFor (o => o.Id, f => f.Random.Number (100))
        .RuleFor (o => o.Name, f => f.Name.FirstName ());

      return new Category () {
        Id = categoryId,
          Name = "MOCK Category",
          LinkContentList = testData2.Generate (20),
          LinkSubCategoryList = testData.Generate (10)
      };
    }

    /// <summary>
    /// 親カテゴリを読み込みます
    /// </summary>
    /// <param name="categoryId">親カテゴリを読み込みたいカテゴリのID</param>
    /// <returns>親カテゴリ</returns>
    public Category LoadParentCategory (long categoryId) {
      this.mlogger.Trace ("EXECUTE");
      return new Category () { Name = "MOCK Category" };
    }
  }
}
