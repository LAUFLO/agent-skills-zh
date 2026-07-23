# 中文 Agent Skills

这个仓库集中存放可复用的中文 Agent Skill。

## 包含的 Skill

### safe-project-delivery

为任意 Git 项目建立分级授权的修改、测试、提交、PR、合并与发布流程，避免普通代码修改被自动扩大为直接推送主分支或发布版本。

### pixel-winforms-ui

为 Windows WinForms 应用建立纯白背景、粗像素边框、统一控件、DPI、多屏和无闪烁刷新规范，并提供可复制的 C# 主题模板。

### auto-gen-testcase-from-req

从系统需求文档生成可追溯、可直接执行的标准 Markdown 功能测试用例。

该目录从本机现有 Skill 原样同步，不在本仓库发布过程中修改其内容。

## 使用方式

将目标 Skill 文件夹复制到本机 Skill 目录，例如：

```text
%USERPROFILE%\.codex\skills\
```

重新打开客户端后即可按 Skill 的触发描述使用。
