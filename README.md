###FAQ 常见问题

C# http 服务器没有权限启动 ， 关掉防火墙， 开启权限

[参考回答](https://stackoverflow.com/questions/4019466/httplistener-access-denied)

Yes you can run HttpListener in non-admin mode. All you need to do is grant permissions to the particular URL. e.g.

管理员权限运行 cmd，接着执行下面命令

 ** netsh http add urlacl url=http://127.0.0.1:12030/ user=你的windows用户名 **



###游戏视频

https://v.qq.com/x/page/w0523vg401v.html


###整个系统包括：

1：Moba 服务器端物理

2：Moba 客户端物理

3：moba 网络同步

4：moba技能系统实现

5：Moba AI 系统

###游戏元素包括：
塔

水晶

小兵

玩家

场景地图

###技术实现:

服务器采用c#实现 + mysql数据库

客户端采用Unity 2017.2.0f3 实现, 纯c#代码


###源码下载：

客户端

https://gitee.com/liyonghelpme/MobaKeHuDuan

服务器

https://gitee.com/liyonghelpme/MobaFuWuQi

网络协议

https://gitee.com/liyonghelpme/mobaXieYi

配置表

https://gitee.com/liyonghelpme/mobaPeiZhi


###交流群：

QQ: 390313628


###部署说明：

Unity 2017.2.0f3 版本

Mysql数据库

.NetFrame 4.6

Visual studio 2015 社区版



1：安装Mysql服务器

参考服务器下的 ServerConfig.json 

创建数据库tank

账号root

密码123456


执行服务器目录下的

tankServer\SocketServer\ConfigData\Scripts\tank.sql  初始化数据库 tank

2：启动服务器工程

Visualstudio 打开tankServer   SocketServer工程开始执行


3：复制两份Unity客户端工程，使用 Unity2017.2.0f3 打开工程

打开两个客户端工程


![输入图片说明](https://gitee.com/uploads/images/2017/1213/012809_032be703_11587.png "图片1.png")
![输入图片说明](https://gitee.com/uploads/images/2017/1213/012826_d27dead4_11587.png "图片2.png")

![输入图片说明](https://gitee.com/uploads/images/2017/1213/012843_c8c290f5_11587.png "图片3.png")
![输入图片说明](https://gitee.com/uploads/images/2017/1213/012851_de37ef70_11587.png "图片4.png")
![输入图片说明](https://gitee.com/uploads/images/2017/1213/012857_2fe2f862_11587.png "图片5.png")
![输入图片说明](https://gitee.com/uploads/images/2017/1213/012907_5208f91b_11587.png "图片6.png")

[![输入图片说明](https://gitee.com/uploads/images/2017/1213/075005_1d3e1cec_11587.png "QQ图片20171213074950.png")](https://v.qq.com/x/page/u05182pooi4.html)



