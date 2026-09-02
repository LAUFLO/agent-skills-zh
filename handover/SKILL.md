---
name: handover
description: 会话接续与项目控制面 skill，让新会话一秒接续。适用于 /handover、会话交接、状态同步、看板更新、任务同步、收工、下一阶段、跨会话记忆、Praxis 交接等场景。当用户说好了/完成了/下一个阶段/收工/同步一下状态/交接 或新会话启动时务必使用。已替代旧 /kanban 命令。
---

# Handover — 会话接续 Skill

> 目标：让新会话一秒接续，不丢进度、不丢踩坑、不丢决策。

## 何时触发
- 新会话启动（AGENTS.md 首行会让你读 boot-packet）
- 用户说 `好了/完成了/先这样/下一个阶段/收工/handover/交接/同步状态`
- 本次会话有推进但尚未执行 handover
- `frontier` 全部 `terminal` 或检测到 `docs/staging/plans/*.md` 有更新

## 数据位置
- **唯一路径**：`.opencode/handover/`（单一真相源，不再分叉）
- 核心文件：`state.json` / `boot-packet.md` / `project_summary.md` / `evidence/` / `lib.mjs`

## 执行步骤（幂等）

### 0. 确定 PROJECT_ROOT
`git rev-parse --git-common-dir` → `.git` 则 `git rev-parse --show-toplevel`，worktree 取父目录，失败回退 `cwd`。

### 1. 读取当前状态
读取 `state.json`（校验 `version`）、`AGENTS.md`，经 `lib.mjs:normalizeLesson` 兼容旧 `lessons`（无 `category` 视为 `engineering`）。

### 1.5 读取可选输入源（Praxis 为可选 provider）
`provider=auto` 默认：检测 `docs/staging/plans/*.md` 或 `docs/tech-spec.md` 是否存在，存在则读，不存在跳过（`lib.mjs:shouldReadPraxis`），不报错。显式 `provider=praxis|none` 可强制。

启用时按需读取：`plans/*.md`（最新）→ `specs/*.md` → `ROADMAP.md` → `decisions/*.md` → `tech-spec.md`。无 Praxis 时 `state.json` 即为真相源。

### 2. 分析本次会话成果
- 2.1 扫描 `完成/阻塞/决策变更` 三类信号，提取文件路径与 `evidence`
- 2.2 `phase ↔ milestone` 以 `state.json` 为准
- 2.3 匹配 `frontier`：主依据对话信号，Praxis 仅为可选补货源；**P0 语义去重** `normalizeContent/contentHash/upsertTodo`，同 `hash` 更新原 todo；新增 `lessons` 必须带 `category: engineering|procedure|preference|repo`
- 2.4 阻塞 → `gates`（scope/resolve/blocked_todos）
- 2.5 决策 → `lessons`
- 2.6 统计消耗
- 2.7 **进展确认（必须等待用户确认后才继续 3）**：展示 ✅terminal / 🚧gate / 🔄lessons / 📋剩余 frontier / 📂Praxis 吸收项

### 3. 更新 state.json（原子写）
按 `phase` 编号与 `todo id` + `contentHash` 去重，`checkVersionConflict` 校验 `version`，`bumpVersion` 递增后原子写入。

### 4. 生成 boot-packet.md（500字内）
含 phase goal、frontier、gates、evidence 摘要、按 `filterLessonsForBoot` 过滤的 3 条 lessons（按 frontier 关键词相关性）、设计约定、Praxis staging 路径。

### 5. 生成 project_summary.md
项目概览、结构、当前进度、已完成摘要、设计约定、staging 决策摘要。

### 6. 更新 AGENTS.md
首行必须 `> **新会话启动时，请首先读取 .opencode/handover/boot-packet.md ...**`，含自动看板规则。

## 进展信号规范
- 完成：动作词 + 文件路径 + 验证方式，证据落 `evidence/{todo-id}_{描述}.{ext}`
- 阻塞：内容 + 原因 + 解除方式
- 决策：原方案 → 新方案 + 原因
