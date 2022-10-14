# MinimalAPIの認証テスト用のモジュールです。



# 環境情報

| 機能 | バージョン |
| ---- | ---- |
| Linux/Ubuntu | 20.4.* |
| .NET | 6.0 |
| C# | .NET依存 |


# 環境構築手順


```bash
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y dotnet-sdk-6.0

dotnet run
```


# 参考文献

- [Ubuntuに.NETをインストールする方法](https://learn.microsoft.com/ja-jp/dotnet/core/install/linux-ubuntu)
- [.NETアプリケーションの実行](https://learn.microsoft.com/ja-jp/troubleshoot/developer/webapps/aspnetcore/practice-troubleshoot-linux/2-1-create-configure-aspnet-core-applications)
- [MinimalAPIの概要](https://learn.microsoft.com/ja-jp/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0)


