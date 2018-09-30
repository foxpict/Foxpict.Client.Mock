using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Foxpict.Client.Sdk.Infra;
using Newtonsoft.Json.Linq;

namespace foxpict.client.web {
  /// <summary>
  ///
  /// </summary>
  public class PseudoFrontentIpcMessageBridge : IFrontendIpcMessageBridge {
    readonly ConcurrentQueue<PseudoNotificationResponseItem> mSendMessageQueue = new ConcurrentQueue<PseudoNotificationResponseItem> ();

    readonly Dictionary<string, Action<object>> mEventHandlerDict = new Dictionary<string, Action<object>> ();

    /// <summary>
    /// IPCメッセージを処理するハンドラを追加します
    /// </summary>
    /// <param name="ipcEventName"></param>
    /// <param name="receiveHandler"></param>
    public void RegisterEventHandler (string ipcEventName, Action<object> receiveHandler) {
      mEventHandlerDict.Add (ipcEventName, receiveHandler);
    }

    /// <summary>
    /// フロントエンドから送信されたIPCメッセージを受け取ります
    /// </summary>
    /// <param name="ipcEventName"></param>
    /// <param name="param"></param>
    public void Send (string ipcEventName, IpcMessage param) {
      mSendMessageQueue.Enqueue (new PseudoNotificationResponseItem () {
        EventName = ipcEventName,
          Data = param
      });
    }

    /// <summary>
    /// IPCメッセージを処理します
    /// </summary>
    /// <param name="ipcMessage"></param>
    /// <param name="param"></param>
    public void Invoke (string ipcMessage, IpcMessage param) {
      if (mEventHandlerDict.ContainsKey (ipcMessage)) {
        mEventHandlerDict[ipcMessage].Invoke (JObject.FromObject (param));
      }
    }

    public List<PseudoNotificationResponseItem> AllResponse () {
      var result = new List<PseudoNotificationResponseItem> (mSendMessageQueue.ToArray ());
      mSendMessageQueue.Clear ();
      return result;
    }

    public int GetPseudoCount () {
      return mSendMessageQueue.Count;
    }
  }

  public class PseudoNotificationResponse {
    public PseudoNotificationResponse (List<PseudoNotificationResponseItem> messages) {
      this.Messages = messages.ToArray ();
    }

    public PseudoNotificationResponseItem[] Messages;
  }

  public class PseudoNotificationResponseItem {
    public string EventName;
    public IpcMessage Data;
  }
}
