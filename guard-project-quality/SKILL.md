---
name: guard-project-quality
description: 为新项目初始化可维护的目录、编码规范、测试与 CI 质量门禁，并审计或整理现有项目的结构、大文件、根目录源码和工程配置。Use when the user asks to create a project, establish coding standards, organize or refactor a repository, split large files, add linting or static analysis, create AGENTS.md/.editorconfig/CI quality rules, or audit project quality; do not use it as a replacement for Git submission, PR, merge, or release workflows.
---

# 项目质量守卫

建立可执行、可审计的项目规范。把格式、静态分析、测试和 CI 作为硬约束，把 Skill 与 `AGENTS.md` 作为工作流程约束。

## 必读资源

- 每次使用都读取 [references/quality-rules.md](references/quality-rules.md)。
- 确认技术栈后读取 [references/stack-profiles.md](references/stack-profiles.md) 中对应章节。
- 需要生成跨 Agent 规则时，复制并改写 [assets/AGENTS_PROJECT_QUALITY_TEMPLATE.md](assets/AGENTS_PROJECT_QUALITY_TEMPLATE.md)。
- 需要建立基础编辑规范时，复制并按技术栈调整 [assets/editorconfig.template](assets/editorconfig.template)。

## 选择工作模式

先根据用户请求选择一种模式，不扩大授权：

- **只读审计**：检查并报告，不修改文件。
- **新项目初始化**：创建目录、配置、最小测试和 CI。
- **现有项目整理**：保留行为和已有约定，分阶段移动源码、拆分大文件并修正构建入口。
- **规范补齐**：只添加缺少的配置、分析器、测试或质量门禁。

用户只要求检查或询问时，保持只读。现有项目的大范围移动应先给出目标结构、受影响入口和验证方法。

## 工作流程

### 1. 盘点项目

1. 查找仓库根目录、`AGENTS.md`、贡献指南、构建文件和 CI。
2. 检查 Git 分支、工作区、暂存区和未跟踪文件；保护用户已有改动。
3. 识别语言、框架、生成目录、依赖目录、入口点和测试命令。
4. 优先延续项目已有的合理约定，不套用无关模板。

### 2. 建立目标结构

- 新项目默认使用 `src`、`tests`、`docs`、`assets` 和 `scripts`，再按技术栈调整。
- 现有项目只在结构收益明确时移动文件；同步更新项目文件、导入路径、构建脚本、测试和文档。
- 将 UI/API、应用逻辑、领域模型和基础设施依赖分开，避免循环依赖。
- 源码原则上不散落仓库根目录；保留技术栈约定的入口和配置文件。

### 3. 建立可执行规范

至少覆盖：

- UTF-8、换行、缩进和尾随空格；
- 格式化、lint 或编译器分析器；
- 单元测试和适用的集成测试；
- CI 中的格式、静态分析、测试和构建检查；
- `AGENTS.md` 中的目录、文件规模、验证与 Git 权限规则。

优先使用生态原生工具，不平行维护重复检查器。

### 4. 控制文件规模

- 超过 500 行时评估职责并提醒拆分。
- 超过 800 行时原则上拆分，除非属于生成代码、声明式数据、迁移、快照或拆分会明显降低可维护性。
- 按职责、生命周期和依赖边界拆分，不为满足行数机械切割。
- 拆分后保证公共 API、序列化格式和用户行为不变。

### 5. 运行审计

运行：

```text
python scripts/audit_project.py <project-root>
```

需要机器可读结果时增加 `--format json`；需要让警告导致失败时增加 `--strict`。先检查报告中的技术栈例外，再决定是否修改。

### 6. 验证

按风险从小到大验证：

1. 格式和静态分析；
2. 受影响测试；
3. 完整测试与构建；
4. 新项目 CI 配置语法；
5. 再次运行结构审计。

报告实际执行的检查，不把未运行的检查写成通过。

### 7. 交付边界

本 Skill 不负责提交、推送、PR、合并、标签或发布。用户要求这些操作时，同时使用 `safe-project-delivery`，并按其授权层级停止。

## 强制规则

- 项目自己的明确规则优先于本 Skill。
- 不移动或格式化无关文件。
- 不修改依赖、构建产物、第三方或生成目录来消除审计告警。
- 不把所有项目强制成同一种目录；遵循语言和框架惯例。
- 不用注释掩盖过度耦合；优先改进边界和命名。
- 不以删除测试、降低分析级别或忽略整个目录的方式让检查变绿。
- 不把此 Skill 与 Git 交付流程合并。

## 结果格式

说明：

- 采用的模式和技术栈；
- 新增或调整的规范；
- 仍需处理的警告及例外理由；
- 实际验证结果；
- 当前停留在只读、已修改未提交，还是已转交安全交付流程。
