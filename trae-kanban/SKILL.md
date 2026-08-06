---
name: "kanban"
description: "更新项目控制面：结构化状态(state.json)、任务看板与启动入口(boot-packet.md)，让新会话一秒接续。当用户说'好了''完成了''先这样''下一个阶段''收工'等结束信号时，或当前frontier所有todo已完成时，或用户明确要求执行看板/总结项目时，主动调用此Skill。"
---

# /看板 — 项目看板 Skill

更新项目控制面，供下次开发接续。保持与 Command 版本完全一致的功能。

## 初始化检查（首次使用自动执行）
- 若项目根目录无 `.trae/` 目录，自动创建以下子目录：
  - `.trae/evidence/`
- 若 `.trae/state.json` 不存在，根据当前项目内容创建初始状态
- 若项目根目录无 `project_memory.md`，创建初始版本

## 执行步骤

### 1. 读取当前状态
- 读取 `.trae/state.json`（如果存在）
- 读取项目根目录 `project_memory.md`（如果存在）

### 2. 分析本次会话成果
- 哪些 todo 完成了？→ 标记 status: "terminal"，附上 evidence（文件路径、测试结果、截图等）
- 哪些是新发现的阻塞？→ 写入 gates，明确 scope、resolve 方式、blocked_todos
- 哪些决策变了？→ 写入 lessons，区分 replan（路线变更）和 self_repair（行为修正）
- 消耗了多少？→ 更新 spend

### 3. 更新 state.json（幂等）
位于 `.trae/state.json`，增量更新，不覆盖历史阶段：

- **按 phase 编号匹配**：先检查 phases 数组中是否已有当前 phase 编号
  - 有 → 更新该 phase 的 frontier、gates、lessons、spend，不新增
  - 无 → 追加新 phase
- **按 todo id 去重**：每个 todo 有唯一 id（如 `todo-1-1`），写入前先检查是否存在
  - 存在 → 更新 status 和 evidence，不重复创建
  - 不存在 → 追加
- **gate 同理**：按 gate id 去重，已存在的只更新 status
- 已完成的任务标记 status: "terminal" 并附带 evidence（至少一条）
- 当前 phase 的 frontier 只保留未完成的 todo
- 新发现的阻塞写入 gates，已解决的标记 status: "resolved"
- 更新 spend 统计（累加，不覆盖）

### 4. 生成 boot-packet.md
位于 `.trae/boot-packet.md`，控制 500 字以内，只包含：
- 当前阶段 goal 和状态
- 可直接推进的 todo（frontier）
- 阻塞中的 todo 及阻塞原因（gates）
- 上次完成的 evidence 摘要
- 关键教训/注意事项（lessons 中最近 3 条）
- 项目设计规范/约定（如有）

### 5. 生成 project_summary.md
位于 `.trae/project_summary.md`，人类可读看板：
- 项目概览和当前阶段
- 项目结构
- 当前阶段进度（frontier + gates）
- 已完成阶段摘要
- 设计规范/约定
- 后续注意事项

### 6. 更新 project_memory.md
位于项目根目录 `project_memory.md`，同时尝试复制到 TRAE 记忆目录下的对应子目录：
- 第一行必须是：`> **新会话启动时，请首先读取 .trae/boot-packet.md 获取当前任务状态。**`
- 必须包含自动看板规则（见下方）
- 后续内容：项目约定、开发流程、稳定不常变的信息
- 不要重复 state.json 或 boot-packet 的内容
- 如果复制到 TRAE 记忆目录失败，告知用户手动复制路径

### 7. project_memory.md 必须包含的自动看板规则
- 当用户说"好了""完成了""先这样""下一个阶段""收工"等结束信号时，主动询问是否需要执行看板
- 当检测到当前 frontier 所有 todo 均为 terminal 状态时，自动提醒用户执行看板
- 每次对话结束前，检查是否有未记录的进展，如有则提醒用户执行看板

## 写入规则
- 所有文件写入后告知用户文件清单
- state.json 是真相源，其他文件是其视图，不可反向修改
- project_memory.md 必须同时存在于项目根目录和 TRAE 记忆目录
- boot-packet.md 和 project_summary.md 每次全量重新生成，天然幂等
- 若 project_memory.md 写入 TRAE 记忆目录失败，告知用户手动复制路径