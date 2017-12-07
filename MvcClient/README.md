
# 简介
本项目使用Identity Server 4建立了一个最简单的ASP.Net Core 2的认证服务。

参考文章：
- [使用Identity Server 4建立Authorization Server (1)](
http://www.cnblogs.com/cgzl/p/7780559.html)


## IdentityServer
认证服务，通过用户名、密码来获取证书，供API Server使用。

## APIServer
提供数据访问的服务。

- GET /api/values   
无认证的接口。

- GET /api/identity  
需要认证以后才能访问的接口

## Client
演示了一个控制台程序如何使用IdentityServer获取`token`，之后使用`token`查询APIServer需要认证的API接口。


## 数字证书
本项目中的数字证书`2018年11月30`日失效，参考原文中有OpenSSL生成数字证书的步骤，按文档重新生成覆盖现有文件即可。