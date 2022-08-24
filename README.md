# 部署手册

## Daemon (守护程序)

- 将程序发布到vs2022/_publish文件夹
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
