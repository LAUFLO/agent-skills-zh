# 常见技术栈配置

只读取并采用与当前项目相关的配置。项目已有成熟约定时优先延续。

| 技术栈 | 推荐结构 | 格式与分析 | 测试与构建 |
|---|---|---|---|
| .NET / C# | `src/<Product>`、`tests/<Product>.Tests`、解决方案文件留在根目录 | `.editorconfig`、`dotnet format`、Roslyn analyzers、nullable | `dotnet test`、`dotnet build -warnaserror`；传统 Framework 项目使用其现有 MSBuild/Roslyn 入口 |
| Node / TypeScript | `src`、`tests` 或框架约定目录，配置文件留根目录 | Prettier、ESLint、TypeScript `noEmit` | 项目包管理器的 test/build；锁定 Node 与包管理器版本 |
| Python | `src/<package>` 优先，简单工具可使用顶层包目录；测试放 `tests` | Ruff、Black 或 Ruff format、mypy/pyright | pytest、构建 wheel/sdist；使用 `pyproject.toml` 集中配置 |
| Java / Kotlin | Maven/Gradle 标准 `src/main`、`src/test` | Spotless、Checkstyle/Detekt、编译器警告 | Maven/Gradle test 与 package/build |
| Go | 单包工具允许根目录 `.go`；多命令使用 `cmd`，内部实现使用 `internal` | gofmt、go vet、staticcheck | `go test ./...`、`go build ./...` |
| Rust | Cargo 标准 `src`、`tests`、`examples` | rustfmt、Clippy | `cargo test`、`cargo build`，CI 可使用 `-D warnings` |

## UI 项目

- 将视图、状态管理、业务规则和系统集成分开。
- 自绘控件、窗口和后台采集各自拥有明确生命周期。
- UI 列表覆盖空、单条、满页、溢出、DPI 和键盘操作。

## Web 项目

- 保持路由、组件、领域逻辑、数据访问和外部客户端边界。
- 校验服务端与客户端输入；不要只依赖前端校验。
- 新增可访问性、响应式和关键用户路径测试。

## CLI 与服务

- 将参数解析、应用服务和外部 I/O 分开。
- 支持非交互运行、明确退出码、超时和取消。
- 配置优先使用环境或配置文件，不硬编码密钥与机器路径。
