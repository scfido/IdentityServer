# 简介

本项目是使用Identity Server 4建立的认证服务。

解决方案中一共4个项目：
- IdentityServer是认证服务
- APIServer 提供了一个需要认证才能访问的API
- ConsoleClient 演示一个无用户参与的客户端，使用AppId和Secret访问API
- MvcClient 演示有用户参与的客户端，除了AppId和Secret以外，还需要用户登陆。

# 使用
## 获取项目
```
git clone http://github.cd.xunmei.com/dev/identityserver.git
cd identityserver
```

## 初始化数据库
首先修改数据库连接字符串，本项目使用的MySQL数据库。

打开`IdentityServer\appsettings.json`，修改`ConnectionStrings.MySQL`的值与你的MySQL服务器一致。
```json
{
  "ConnectionStrings": {
    "MySQL": "Server=localhost;database=ids;uid=root;pwd=123456;"
  }
}
```

进入IdentityServer项目的目录, 使用`dotnet ef`生成数据库合并脚本。  
![](/assets/drafts/README/img/2017-12-14-16-42-41.png)

如果提示命令不存在则需要手动编辑`IdentityServer.csproj`文件，加入以下内容。
```xml
<ItemGroup>
    <DotNetCliToolReference Include="Microsoft.EntityFrameworkCore.Tools.DotNet" Version="2.0.0" />
</ItemGroup>
```
这里一共有3个命令(migrations), 第一个是为了IdentityServer的配置, 第二个是为了持久化授权，第三个是ASP.NET Identity的用户数据库.
```
dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/IdentityServer/PersistedGrantDb
dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/IdentityServer/ConfigurationDb
dotnet ef migrations add InitialIdentityServerApplicationDbMigration -c ApplicationDbContext -o Data/Migrations
```

## 生成数字证书

证书可以通过几种渠道获得, 可以购买, 可以使用IIS生成, 也可以使用Openssl这样的工具生成证书. 我就使用openssl吧.
去openssl的windows官网: https://slproweb.com/products/Win32OpenSSL.html

下载 1.1.0版: https://slproweb.com/download/Win64OpenSSL-1_1_0f.exe

安装后, 打开命令行.
```
openssl req -newkey rsa:2048 -nodes -keyout socialnetwork.key -x509 -days 365 -out socialnetwork.cer
```
具体的信息就不管了. 这个证书的有效期是365天, 命令参数里面设定的.

这是生成的文件:  
![](/assets/drafts/README/img/2017-12-14-16-34-10.png)


一个证书和一个key, 然后我们需要给他们俩封装成一个文件, 以便identity server可以使用它们去正确的签名tokens. 这就需要使用另一个命令:
```
openssl pkcs12 -export -in socialnetwork.cer -inkey socialnetwork.key -out socialnetwork.pfx
```
![](/assets/drafts/README/img/2017-12-14-16-35-55.png)

这里发生了错误...那就使用管理员打开命令行:

![](/assets/drafts/README/img/2017-12-14-16-37-55.png)

输入密码和确认密码后, 没问题了.

![](/assets/drafts/README/img/2017-12-14-16-38-10.png)

pfx就是我们需要的文件.

然后修改`IdentityServer\appsettings.json`中的`auth.certifcate`对应参数。

```json
{
    "auth": {
        "certifcate": {
            "name": "socialnetwork.pfx",
            "password": "11111"
        }
    }
}
```

## 初始化配置
通过上述操作系统已经配置完毕，下一步我们要添加测试数据，以便Client认证和用户登录。

进入`IdentityServer`项目目录使用`/seed`参数启动初始化测试数据。
```
dotnet run -- /seed
```

## 启动API Server
启动API Server供后续测试使用。
进入`APIServer`目录
```
dotnet run
```

## 测试Console Client
启动Identity Server后，就可以使用Console Client测试了。

进入`ConsoleClient`目录
```
dotnet run
```

正常可以看到类似以下输出。
![](/assets/drafts/README/img/2017-12-14-17-10-22.png)

## 测试MVC Client
MVC客户端就是一个网站，先运行起来。 
进入`MvcClient`目录
```
dotnet run
```
启动MVC网站后测试步骤如下： 
1. 首先在Identity Server上注册一个用户
2. 点击MvcClient网站首页的`About`链接
3. 自动跳转到Identity Server网站，输入注册的用户名登陆按步骤操作，成功后会自动转回MvcClient网站。
4. 点击MvcClient网站首页的`API`链接
5. 显示从APIServer获取到的数据

