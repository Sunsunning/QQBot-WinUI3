# QQBot-WinUI3

#### QQ机器人
基于LLOneBot的聊天机器人，为实现自定义QQ机器人提供了便捷的方法，本软件提供了插件接口可进行自定义命令并且你也可以自由选择机器人已预设的功能!

#### 使用教程
- 准备工作  
前提:已安装 **LiteLoaderQQNT** 并已安装 **LLOneBot插件** ，若无请[查看此文档进行安装](https://llob.napneko.com/zh-CN/guide/getting-started)  
1.打开QQNT设置页面设置" **HTTP服务监听端口** "与" **启用HTTP事件上报** "并把" **HTTP事件上报"-"**  **HTTP事件上报地址** "设置为"http://localhost:端口号"  
2.没了
- 创建机器人  
1.打开软件切换到 **添加机器人页面**   
2.将 **IP地址** 改为你需要的地址并将 **服务监听端口** 设置为刚刚设置的 **HTTP服务监听端口** 将 **事件上报端口** 设置为刚刚设置的 **HTTP上报地址中的端口号**  
3.配置你需要的功能  
4.点击提交，此时机器人应该会正常创建  
- 启动机器人  
1.打开 **机器人列表** 页面即可启动，这不会可以别看了()
- 装载插件
1.打开 **机器人列表** 页面点击 **打开**  按钮即可打开机器人配置所在目录找到 **plugins** 文件夹放入插件(通常为dll后缀)重启机器人即可装载插件
- 控制台命令  
1.stop - 关闭机器人  
2.restart - 重启机器人  
3.clear - 清空历史记录  
4.send [QQ群号] [消息] - 操控机器人发送消息  
- 偷鸡摸狗的事 
 _机器人每次启动时都会产生日志文件可在 **机器人目录** /logs下找到你可以偷偷看他们撤回的消息哦_ 

#### 创建你的插件
- 准备工作 请现在本仓库 **发行版** 中的 **预览** 版本中下载 **IPlugin.dll** 文件并将其添加应用到你的插件项目中
- 开始  
1.创建项目选择 **C#类库** 并将框架设置为 **.NET 8.0**   
2.在 **解决方案视图**中找到 **依赖项** 右键他并在选择 **添加项目引用** 选择 **浏览** 并找到下载的 **IPlugin.dll** 将其添加到项目依赖中  
#### 编写代码
在正式编写代码开始先你需要继承IPlugin接口已实现与宿主程序的正确互通所以你需要添加以下引用

```
using static PluginDLL.Class1;
```
并将你的类继承IPlugin接口例如

```
 public class ExamplePlugin : IPlugin{}
```
接下来你需要实现他接口的所有方法，以下是所有方法的介绍  
-  **事件加载顺序** - 先进行 **SetHostService()** 后进行 **OnEnable()**   
-  **description()**  - 用于机器人在接收到/help后发送的命令列表输出的所有命令以及描述，不需要则输入以下代码
    ```
     public List<string> description()
     {
         return new List<string>(); 
     }
    ```
-  **GroupMessageReceived(MessageEvent e)**  - 机器人在接收到群聊消息时触发
-  **PrivateMessageReceived(MessageEvent e)**  - 机器人在接收私聊消息时触发
-  **OnEnable()** - 机器人在加载时触发
-  **SetHostService(ILoggerService loggerService, IMessageService messageService, IConfigService configService, string url)** - 用于机器人初始化，以下是参数解释  
 _loggerService_  - 用于发送控制台日志消息,调用方法
    
    ```
    loggerService.LogMessage("info", "msg");
    loggerService.LogMessage("error", "msg");
    loggerService.LogMessage("waring", "msg");
    loggerService.LogMessage("debug", "msg");
    ```
   _messageService_ - 用于机器人发送消息，调用方法请见下文  
   _configService_ - 用于创建机器人配置文件，调用方法
    
    ```
    string path = configService.getCurrentDirectory();
    configService.DeleteConfig("FolderName", "FileName");
    configService.CreateConfig("FolderName", "FileName");
    ```
   其中FolderName是插件名称或其他名称，无论如何插件配置文件都将创建在./plugins/FolderName/下  
   _url_ - 机器人的事件上报地址你可以通过此地址发送消息，而不依赖与 _messageService_ （局限性太多了,其实是我一开始写实现就没写好懒得改了(）发送自定义消息(以下引用了 **Newtonsoft.Json** 库

    ```
     public class ExamplePlugin: IPlugin
     {
         public async void GroupMessageReceived(MessageEvent e)
         {
            await sendMessage(e.GroupId,"msg","imgurl","summary");
         }
        
         public async Task<string> sendMessage(long groupId,  string text, string imageUrl, string summary, bool sendToConsole = true)
         {
             var messageData = new JObject
             {
                 ["group_id"] = groupId,
                 ["message"] = new JArray
                 {
                     new JObject
                     {
                         ["type"] = "image",
                         ["data"] = new JObject
                         {
                             ["summary"] = summary,
                             ["subType"] = 0,
                             ["url"] = imageUrl
                         }
                     },
                     new JObject
                     {
                         ["type"] = "text",
                         ["data"] = new JObject
                         {
                             ["text"] = text
                         }
                     }
                 }
             };
        
             string json = JsonConvert.SerializeObject(messageData);
             var content = new StringContent(json, Encoding.UTF8, "application/json");
             return await SendPostRequestAsync($"{serviceAddress}send_group_msg", content, sendToConsole);
         } 
     }
    ```
#### messageService调用方法

```
     Task<string> SendGroupDirectMessageAsync(long groupId, string message, bool autoEscape = false, bool sendToConsole = true);
    
     Task<string> SendGroupMessageAsync(long groupId, long message_id, string message, bool autoEscape = false, bool sendToConsole = true);
    
     Task<string> SendGroupImageMessageAsync(long groupId, long message_id, string url, string summary, bool sendToConsole = true);
    
     Task<string> SendGroupVoiceMessageAsync(long groupId, string url, bool sendToConsole = true);
    
     Task<string> SendGroupMessageAsync(long groupId, long message_id, string text, string imageUrl, long at_userId, string name, string summary, bool sendToConsole = true);
    
     Task<string> SendPrivateMesageDirectMessageAsync(long user_id, string message, bool autoEscape = false, bool sendToConsole = true);
    
     Task<string> SendPrivateMessageAsync(long user_id, long message_id, string message, bool autoEscape = false, bool sendToConsole = true);
    
     Task<string> SendPrivateImageMessageAsync(long user_id, long message_id, string url, string summary, bool sendToConsole = true);
    
     Task<string> SendPrivateVoiceMessageAsync(long user_id, long url, bool sendToConsole = true);
    
     Task<string> SendPrivateMessageAsync(long user_id, long message_id, string text, string imageUrl, long at_userId, string name, string summary, bool sendToConsole = true);
    
     Task<string> sendLike(long user_id, int times, bool sendToConsole = true);
```

芝士源接口的方法如需调用则为 messageService.SendGroupDirectMessageAsync(arguments);
- 参数解释  
 **groupId** - QQ群号，获取则需要从GroupMessageReceived中接收到的MessageEvent e参数中获取例如`long id = e.GroupId;`  
     **user_id** - 私聊对话发送者QQ号，获取需要从PrivateMessageReceived中接收到的MessageEvent e参数中获取例如`long id = e.Sender.UserId;`  

     **message_id** - 用于回复消息，获取示例`long msg_id = e.MessageId`  

     **message** - 发送的消息  

     **text** - 与messsage相同  

     **imageUrl** **url** - 在不同场景有不同意思在image发送时是 **图片地址** 或 **文件路径** 在语音发送时候指  **语音地址** 或 **文件路径**  

     **at_userId** **name** - at_userId为发送消息者QQ号与 **user_id ** 获取方式相同  **name** 则为消息发送者的 **nickname** 获取示例`string nickname = e.Sender.Nickname;`  

     **summary** - 图片发送时在消息弹窗或窗口预览中显示文本，随便填写

     **times** - 点赞次数

     **sendToConsole** - 聊天发送时是否返回给控制台



#### 参与贡献
就我自己
