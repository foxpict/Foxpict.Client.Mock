using System;
using System.IO;
using Foxpict.Client.Sdk;
using Foxpict.Client.Sdk.Bridge;
using Foxpict.Client.Sdk.Core.Intent;
using Foxpict.Client.Sdk.Core.IpcApi;
using Foxpict.Client.Sdk.Core.ServerMessageApi;
using Foxpict.Client.Sdk.Core.Service;
using Foxpict.Client.Sdk.Infra;
using Foxpict.Client.Sdk.Intent;
using Foxpict.Client.Sdk.Models;
using Foxpict.Client.Web.Dao;
using Foxpict.Client.Web.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;
using Swashbuckle.AspNetCore.Swagger;

namespace foxpict.client.web {
  public class Startup {
    private Container mContainer = new Container ();

    private Logger mLogger = LogManager.GetCurrentClassLogger ();

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="configuration"></param>
    public Startup (IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices (IServiceCollection services) {
      services.AddMemoryCache ();
      services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_1);
      services.AddSwaggerGen (c => {
        c.SwaggerDoc ("v1", new Info {
          Version = "v1",
            Title = "Foxpict.Client.Web API",
            Description = "A simple example ASP.NET Core Web API",
            Contact = new Contact { Name = "Juan García Carmona", Email = "d.jgc.it@gmail.com", Url = "https://wisegeckos.com" },
        });
        // Set the comments path for the Swagger JSON and UI.
        var basePath = AppContext.BaseDirectory;
        var xmlPath = Path.Combine (basePath, "Foxpict.Client.Web.xml");
        c.IncludeXmlComments (xmlPath);
      });
      services.AddCors ();
      IntegrateSimpleInjector (services);
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="env"></param>
    public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
      mLogger.Info ("Starting Mock");

      InitializeContainer (app);
      StartApplication ();

      if (env.IsDevelopment ()) {
        app.UseDeveloperExceptionPage ();
      } else {
        app.UseHsts ();
      }

      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger ();

      // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
      app.UseSwaggerUI (c => {
        c.SwaggerEndpoint ("/swagger/v1/swagger.json", "My API V1");
      });

      app.UseCors (builder => builder
        .AllowAnyOrigin ()
        .AllowAnyMethod ()
        .AllowAnyHeader ());

      app.UseHttpsRedirection ();
      app.UseMvc ();
    }

    private void StartApplication () {
      var appConfig = new AppSettings ();
      var mockAppConfig = new MockAppSettings ();
      Configuration.Bind ("AppSettings", appConfig);
      Configuration.Bind ("MockAppSettings", mockAppConfig);
      Configuration.Bind (mockAppConfig);

      if (!string.IsNullOrEmpty (mockAppConfig.ENV_SERVICESERVER_URL)) {
        // MOCK限定
        // サービスサーバのURLが設定されている場合は、DAOが使用するサービスサーバのURLとして設定する。
        appConfig.ServiceServerUrl = mockAppConfig.ENV_SERVICESERVER_URL;
      }

      mContainer.RegisterInstance (appConfig);
      mContainer.RegisterInstance (mockAppConfig);

      // 設定のダンプ
      mLogger.Info ($"Settings: ServiceServerUrl={appConfig.ServiceServerUrl}");
      mLogger.Info ($"Settings: EnableMockDao={mockAppConfig.EnableMockDao}");

      // Ipcマネージャの初期化
      var frontendIpcMessageBridge = new PseudoFrontentIpcMessageBridge ();
      var ipcBridge = new IpcBridge (mContainer, frontendIpcMessageBridge);
      mContainer.RegisterInstance<IRequestHandlerFactory> (ipcBridge.Initialize ());

      // ServiceDistorionマネージャの初期化
      mContainer.Register<IServiceDistoributor, ServiceDistoributionManager> (Lifestyle.Singleton);

      // Intentマネージャの初期化
      mContainer.RegisterSingleton<IIntentManager, IntentManager> ();

      // Screenマネージャの初期化
      mContainer.RegisterSingleton<IScreenManager, ScreenManager> ();

      // Ipcメッセージブリッジの初期化
      mContainer.RegisterInstance<IFrontendIpcMessageBridge> (frontendIpcMessageBridge);

      // 各種HandlerFactoryの登録
      mContainer.RegisterInstance<ServiceDistributionResolveHandlerFactory> (new ServiceDistributionResolveHandlerFactory (mContainer));
      mContainer.RegisterInstance<ServiceMessageResolveHandlerFactory> (new ServiceMessageResolveHandlerFactory (mContainer));
      mContainer.RegisterInstance<IpcSendResolveHandlerFactory> (new IpcSendResolveHandlerFactory (mContainer));

      if (mockAppConfig.EnableMockDao) {
        IntegrateMockDao ();
      } else {
        IntegrateDao ();
      }

      mContainer.Verify ();

      mContainer.GetInstance<WorkflowService.Handler> ().Initialize (); // 手動での初期化
    }

    private void IntegrateSimpleInjector (IServiceCollection services) {
      mContainer.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle ();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor> ();
      services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, QueuedHostedService> ();
      services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue> ();

      services.AddSingleton<IControllerActivator> (new SimpleInjectorControllerActivator (mContainer));
      services.AddSingleton<IViewComponentActivator> (new SimpleInjectorViewComponentActivator (mContainer));

      services.EnableSimpleInjectorCrossWiring (mContainer);
      services.UseSimpleInjectorAspNetRequestScoping (mContainer);
    }

    private void InitializeContainer (IApplicationBuilder app) {
      // Add application presentation components:
      mContainer.RegisterMvcControllers (app);
      mContainer.RegisterMvcViewComponents (app);

      // Add application services. For instance:
      //container.Register<IUserService, UserService>(Lifestyle.Scoped);

      // Cross-wire ASP.NET services (if any). For instance:
      mContainer.CrossWire<ILoggerFactory> (app);

      // NOTE: Do prevent cross-wired instances as much as possible.
      // See: https://simpleinjector.org/blog/2016/07/

      var queue = app.ApplicationServices.GetService<IBackgroundTaskQueue> (); // ASPNETに登録したサービスのインスタンスを取得する
      mContainer.RegisterInstance<IBackgroundTaskQueue> (queue); // サービスオブジェクトを、他のオブジェクトにインジェクションするためにDIに登録する

      var memCache = app.ApplicationServices.GetService<IMemoryCache> (); // ASPNETに登録したサービスのインスタンスを取得する
      mContainer.RegisterInstance<IMemoryCache> (memCache);
    }

    private void IntegrateDao () {
      mContainer.Register<ICategoryDao, Foxpict.Client.Sdk.Dao.CategoryDao> ();
      mContainer.Register<IContentDao, Foxpict.Client.Sdk.Dao.ContentDao> ();
      mContainer.Register<ILabelDao, Foxpict.Client.Sdk.Dao.LabelDao> ();
    }

    private void IntegrateMockDao () {
      mLogger.Info("MockDAOを使用します");
      mContainer.Register<ICategoryDao, CategoryDao> ();
      mContainer.Register<IContentDao, ContentDao> ();
      mContainer.Register<ILabelDao, LabelDao> ();
    }
  }
}
