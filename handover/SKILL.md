---
name: handover
description: 会话接续与项目控制面 skill，让新会话一秒接续。适用于 /handover、会话交接、状态同步、看板更新、任务同步、收工、下一阶段、跨会话记忆等场景。当用户说好了/完成了/下一个阶段/收工/同步一下状态/交接 或新会话启动时务必使用。
---

# Handover — 会话接续 Skill

> 目标：让新会话一秒接续，不丢进度、不丢踩坑、不丢决策。

## 何时触发
- 新会话启动（AGENTS.md 首行会让你读 boot-packet）
- 用户说 `好了/完成了/先这样/下一个阶段/收工/handover/交接/同步状态`
- 本次会话有推进但尚未执行 handover
- `frontier` 全部 `terminal`，或按 `AGENTS.md ## 状态与工件路径约定` 声明的输入源有更新

## 数据位置
- **唯一路径**：`.opencode/handover/`（单一真相源，不再分叉）
- 核心文件：`state.json` / `boot-packet.md` / `project_summary.md` / `evidence/`

## 执行步骤（幂等）

### 0. 确定 PROJECT_ROOT
`git rev-parse --git-common-dir` → `.git` 则 `git rev-parse --show-toplevel`，worktree 取父目录，失败回退 `cwd`。

### 1. 读取当前状态
读取 `state.json`（校验 `version`）、`AGENTS.md`，经 `normalizeLesson` 兼容旧 `lessons`（无 `category` 视为 `engineering`）。

### 1.5 读取可选输入源（动态发现，不硬编码）
先读 `AGENTS.md ## 状态与工件路径约定`，按其声明的输入源路径读取（如 Spec 工件、工单目录、领域文档）。声明缺失则跳过，不报错、不回退硬编码路径。无声明输入源时 `state.json` 即为真相源。

### 2. 分析本次会话成果
- 2.1 扫描 `完成/阻塞/决策变更` 三类信号，提取文件路径与 `evidence`
- 2.2 `phase ↔ milestone` 以 `state.json` 为准
- 2.3 匹配 `frontier`：主依据对话信号；**语义去重** `contentHash/upsertTodo`，同 `hash` 更新原 todo；新增 `lessons` 必须带 `category: engineering|procedure|preference|repo`
- 2.4 阻塞 → `gates`（scope/resolve/blocked_todos）
- 2.5 决策 → `lessons`
- 2.6 统计消耗
- 2.7 **进展确认（必须等待用户确认后才继续 3）**：展示 ✅terminal / 🚧gate / 🔄lessons / 📋剩余 frontier

### 3. 更新 state.json（原子写）
按 `phase` 编号与 `todo id` + `contentHash` 去重，`checkVersionConflict` 校验 `version`，`bumpVersion` 递增后原子写入。

### 4. 生成 boot-packet.md（500字内）
含 phase goal、frontier、gates、evidence 摘要、按 `filterLessonsForBoot` 过滤的 3 条 lessons（按 frontier 关键词相关性）、设计约定、输入源摘要（实际读到什么写什么，不预设字段名）。`boot-packet.md` 是只读投影，禁止反写 `state.json`。

### 5. 生成 project_summary.md
项目概览、结构、当前进度、已完成摘要、设计约定、输入源决策摘要（实际读到什么写什么）。`project_summary.md` 是只读投影，禁止反写 `state.json`。

### 6. 更新 AGENTS.md
首行必须 `> **新会话启动时，请首先读取 .opencode/handover/boot-packet.md ...**`，含自动接续规则；如缺 `## 状态与工件路径约定` 则补上，不改动工作流描述原文。

## 不变量（仿 LoopX）
- `state.json` 是唯一真相源；`boot-packet.md` / `project_summary.md` 是投影，只读派生。
- 本 skill 不拥有工作流路由权，不硬编码任何工作流路径；路径一律来自 `AGENTS.md ## 状态与工件路径约定`。
- `version` 冲突 fail-closed：先 `checkVersionConflict`，再 `bumpVersion` 原子写。

## 进展信号规范
- 完成：动作词 + 文件路径 + 验证方式，证据落 `evidence/{todo-id}_{描述}.{ext}`
- 阻塞：内容 + 原因 + 解除方式
- 决策：原方案 → 新方案 + 原因
