# 中文 Agent Skills

这个仓库集中存放可复用的中文 Agent Skill。

## 包含的 Skill

### pixel-winforms-ui

为 Windows WinForms 应用建立纯白背景、粗像素边框、统一控件、DPI、多屏和无闪烁刷新规范，并提供可复制的 C# 主题模板。

### auto-gen-testcase-from-req

从系统需求文档生成可追溯、可直接执行的标准 Markdown 功能测试用例。

该目录从本机现有 Skill 原样同步，不在本仓库发布过程中修改其内容。

### handover

会话接续与项目控制面 Skill（原 trae-kanban 重构），让新会话一秒接续。适用于 `/handover`、会话交接、状态同步、看板更新、收工等场景。

- 单一路径 `.opencode/handover/`，单一真相源 `state.json`
- 语义去重 `contentHash`、版本原子写、按 `frontier` 关键词过滤 `lessons`
- 输入源动态发现（路径来自 AGENTS.md 约定）；boot-packet/project_summary 只读投影不反写

核心机制：Frontier（待办）+ Gate（阻塞）+ Evidence（证据），配合进展信号规范，支持跨 `worktree` 与多项目复用。

## 使用方式

将目标 Skill 文件夹复制到本机 Skill 目录，例如：

```text
%USERPROFILE%\.codex\skills\
```

重新打开客户端后即可按 Skill 的触发描述使用。
