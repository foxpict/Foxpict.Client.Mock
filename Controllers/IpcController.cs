using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foxpict.Client.Sdk.Infra;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;

namespace foxpict.client.web.Controllers {
  /// <summary>
  /// IPCメッセージを擬似通知イベントとして取得・送信するRESTコントローラです
  /// </summary>
  [Route ("api/[controller]")]
  [ApiController]
  public class IpcController : ControllerBase {
    private Logger mLogger = LogManager.GetCurrentClassLogger ();

    readonly PseudoFrontentIpcMessageBridge mPseudoFrontentIpcMessageBridge;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="frontendIpcMessageBridge"></param>
    public IpcController (IFrontendIpcMessageBridge frontendIpcMessageBridge) {
      this.mPseudoFrontentIpcMessageBridge = (PseudoFrontentIpcMessageBridge) frontendIpcMessageBridge;
    }

    /// <summary>
    /// IPCメッセージを送信する
    /// </summary>
    /// <returns></returns>
    [HttpPost ("send/{ipcMessageName}")]
    public IActionResult Send (string ipcMessageName, [FromBody] IpcMessage param) {
      mLogger.Trace ($"IN - IpcMessageNane={ipcMessageName}");
      this.mPseudoFrontentIpcMessageBridge.Invoke (ipcMessageName, param);
      mLogger.Trace ("OUT");
      return Ok ();
    }

    /// <summary>
    /// Intentメッセージを送信します。
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    [HttpPost ("sendIntent")]
    public IActionResult SendIntent ([FromBody] IntentMessage param) {
      IpcMessage ipcMessage = new IpcMessage () { Body = JsonConvert.SerializeObject (param) };
      this.mPseudoFrontentIpcMessageBridge.Invoke ("PIXS_INTENT_MESSAGE", ipcMessage);
      return Ok ();
    }

    /// <summary>
    /// 疑似通知メッセージ一覧を取得する
    /// </summary>
    [HttpPost ("intermittent")]
    public ActionResult<PseudoNotificationResponse> Intermittent () {
      if (mPseudoFrontentIpcMessageBridge.GetPseudoCount () > 0) {
        mLogger.Info ("IpcMessageCount=" + mPseudoFrontentIpcMessageBridge.GetPseudoCount ());
      }
      var response = new PseudoNotificationResponse (mPseudoFrontentIpcMessageBridge.AllResponse ());
      return Ok (response);
    }
  }
}
