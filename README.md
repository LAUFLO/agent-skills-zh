# 中文 Agent Skills

这个仓库集中存放可复用的中文 Agent Skill。

## 包含的 Skill

### pixel-winforms-ui

为 Windows WinForms 应用建立纯白背景、粗像素边框、统一控件、DPI、多屏和无闪烁刷新规范，并提供可复制的 C# 主题模板。

### auto-gen-testcase-from-req

从系统需求文档生成可追溯、可直接执行的标准 Markdown 功能测试用例。

该目录从本机现有 Skill 原样同步，不在本仓库发布过程中修改其内容。

### trae-kanban

TRAE 项目看板系统（Command + Skill 双版本），借鉴 LoopX 控制面理念，为分阶段开发提供结构化状态追踪。一条命令即可生成：

- 结构化状态文件（state.json），增量更新，不覆盖历史
- 启动入口（boot-packet.md），新会话一秒接续
- 人类可读看板（project_summary.md）
- 项目记忆（project_memory.md），自动跨会话加载

核心机制：Frontier（待办）+ Gate（阻塞）+ Evidence（证据），配合进展信号规范，让 AI 在开发过程中自然输出可被看板提取的进展信息。

## 使用方式

将目标 Skill 文件夹复制到本机 Skill 目录，例如：

```text
%USERPROFILE%\.codex\skills\
```

重新打开客户端后即可按 Skill 的触发描述使用。
