# 部署手册

## Daemon (守护程序)

- 运行vs2022/publish.bat
- 在Dockerfile所在的路径，执行以下命令
  ```bash
  make docker
  ```

# 使用手册

## Daemon (守护程序)

### Windows

- 以管理员模式运行PowerShell
  ```
  New-Service -Name "FMP" -BinaryPathName '"FMP.exe"'
  ```
