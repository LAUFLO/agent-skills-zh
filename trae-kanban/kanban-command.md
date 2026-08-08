# /看板 — TRAE 项目看板（Command + Skill 双版本）

> 借鉴 LoopX 控制面理念，一条命令让 TRAE 项目在分阶段开发中实现「关闭会话不丢状态，新开会话一秒接续」。

## 两种使用方式

| 方式 | 触发 | 适用场景 |
|------|------|---------|
| **Command** | 用户输入 `/看板` | 手动触发，阶段结束时使用 |
| **Skill** | AI 自动调用或用户说"执行看板" | 配合自动看板规则，AI 可主动触发 |

> 两种方式功能完全一致，推荐两者都配置：Skill 用于自动触发，Command 作为手动备选。

## Skill 版（推荐）

将 `SKILL.md` 放到项目 `.trae/skills/kanban/` 目录下即可。

文件路径：`.trae/skills/kanban/SKILL.md`

```markdown
---
name: "kanban"
description: "更新项目控制面：结构化状态(state.json)、任务看板与启动入口(boot-packet.md)，让新会话一秒接续。当用户说'好了''完成了''先这样''下一个阶段''收工'等结束信号时，或当前frontier所有todo已完成时，或用户明确要求执行看板/总结项目时，主动调用此Skill。"
---

# /看板 — 项目看板 Skill

更新项目控制面，供下次开发接续。

（执行步骤与 Command 版完全一致，见下方）
```

> 完整 Skill 文件见本仓库 [SKILL.md](./SKILL.md)

## Command 版

在 TRAE 中创建自定义命令，名称为 `看板`，内容如下：

```markdown
--- Command Name: 看板 ---
--- Description: 更新项目控制面：结构化状态、任务看板与启动入口，让新会话一秒接续。 ---

请总结当前项目，并更新项目控制面，供下次开发接续。

## 初始化检查（首次使用自动执行）
- 若项目根目录无 `.trae/` 目录，自动创建以下子目录：
  - `.trae/evidence/`
- 若 `.trae/state.json` 不存在，根据当前项目内容创建初始状态
- 若项目根目录无 `project_memory.md`，创建初始版本

## 执行步骤

### 0. 确定项目根目录（必须首先执行）
- 执行 `git rev-parse --git-common-dir` 获取 git 公共目录路径
- 若结果为 `.git`（相对路径）→ 当前处于主仓库，执行 `git rev-parse --show-toplevel` 获取项目根目录
- 若结果为绝对路径（如 `C:/Users/.../StartUpilot/.git`）→ 当前处于 worktree，取其父目录作为项目根目录
- 若命令失败（非 git 仓库）→ 回退使用当前终端 cwd
- 将最终路径记为 `PROJECT_ROOT`，后续所有文件读写操作均基于此路径，不得直接使用终端 cwd

### 1. 读取当前状态
- 读取 `.trae/state.json`（如果存在）
- 读取项目根目录 `project_memory.md`（如果存在）

### 2. 分析本次会话成果

#### 2.1 扫描对话中的进展信号
从对话历史中逐条回溯，提取三类信号：
- **完成信号**：AI 输出中包含"完成""修复""搞定""已实现""已创建""已修改""已删除""已通过""已部署""已合并"等表述的句子，提取对应的文件路径、命令结果、截图路径作为 evidence
- **阻塞信号**：AI 输出中包含"遇到问题""无法继续""需要等""阻塞""暂时跳过""缺少权限""需要确认""API 未就绪""被卡住""先跳过""暂缓"等表述的句子
- **决策变更**：用户或 AI 明确提出的方案变更、方向调整、技术选型变化，包含"改为""改用""之前计划用""换方案"等关键词

#### 2.2 匹配 frontier 中的 todo
- 将完成信号与 state.json 当前 phase 的 frontier 逐条匹配
- 匹配依据：进展中提到的文件路径、功能描述、关键词与 todo 的 content 字段做关联
- 匹配成功 → 标记 status: "terminal"，附上 evidence（至少一条）
- 无法匹配的信号 → 记录为 frontier 之外的额外工作，单独列出待用户确认

#### 2.3 识别新阻塞
- 将阻塞信号整理为 gate，每个 gate 必须明确：
  - scope：阻塞影响的范围
  - resolve：解除阻塞的具体方式
  - blocked_todos：被阻塞的 todo id 列表

#### 2.4 提取决策变更
- 写入 lessons，区分：
  - replan：路线变更（方案、技术选型、架构方向调整）
  - self_repair：行为修正（Bug 修复方式、流程优化）

#### 2.5 统计消耗
- 统计本次会话的工具调用次数、实际产出（新增/修改文件数、代码行数估算）

#### 2.6 进展确认（必须执行）
- 在正式更新 state.json 之前，向用户展示提取结果摘要：
  - ✅ 确认完成：列出将要标记为 terminal 的 todo 及 evidence
  - 🚧 确认阻塞：列出新发现的 gate
  - 🔄 确认决策：列出新增的 lessons
- 等待用户确认后，再继续步骤3

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
位于项目根目录 `project_memory.md`，同时尝试复制到 TRAE 记忆目录下的对应子目录（路径：`c:\\Users\\Administrator\\.trae-cn\\memory\\projects\\` 下与当前工作目录对应的子目录）：
- 第一行必须是：`> **新会话启动时，请首先读取 .trae/boot-packet.md 获取当前任务状态。**`
- 必须包含自动看板规则（见下方）
- 后续内容：项目约定、开发流程、稳定不常变的信息
- 不要重复 state.json 或 boot-packet 的内容
- 如果复制到 TRAE 记忆目录失败，告知用户手动复制路径

### 7. project_memory.md 必须包含的自动看板规则
- 当用户说"好了""完成了""先这样""下一个阶段""收工"等结束信号时，主动询问是否需要执行 /看板
- 当检测到当前 frontier 所有 todo 均为 terminal 状态时，自动提醒用户执行 /看板
- 每次对话结束前，检查是否有未记录的进展，如有则提醒用户执行 /看板

### 8. 进展信号规范（写入 project_memory.md）
AI 在开发过程中应遵守以下进展信号规范，确保输出可被看板命令准确提取。此规范需写入 project_memory.md 的"开发流程"章节。

#### 完成信号
每完成一个子任务时，输出中应自然包含：
- 动作词（"完成""修复""实现""创建""删除""重构""部署""合并"）+ 具体内容
- 至少一个文件路径
- 验证方式（测试通过 / 截图 / 命令输出）
- **证据落地**：将关键验证结果存入 `.trae/evidence/`，文件名格式：`{todo-id}_{描述}.{扩展名}`（如 `todo-1-3_navbar-fix.png`、`todo-2-1_test-pass.txt`），便于新会话 AI 精确定位验证

#### 阻塞信号
遇到无法继续的问题时，输出中应自然包含：
- 阻塞的具体内容
- 阻塞原因
- 解除方式

#### 决策变更信号
方案或方向变化时，输出中应自然包含：
- 原方案 → 新方案
- 变更原因

#### 部分完成信号
一个任务只完成了一部分时，输出中应自然包含：
- 已完成的部分 + 文件路径
- 剩余部分 + 下一步动作

## 写入规则
- 所有文件写入后告知用户文件清单
- state.json 是真相源，其他文件是其视图，不可反向修改
- project_memory.md 必须同时存在于项目根目录和 TRAE 记忆目录
- boot-packet.md 和 project_summary.md 每次全量重新生成，天然幂等
- 若 project_memory.md 写入 TRAE 记忆目录失败，告知用户手动复制路径

--- End Command ---
```

## 文件结构

```
项目根目录/
├── project_memory.md          ← 指针 + 约定（稳定，不随会话变化）
├── .trae/
│   ├── state.json             ← 真相源（结构化，增量更新）
│   ├── boot-packet.md         ← 薄启动入口（< 500 字）
│   ├── project_summary.md     ← 人类可读看板
│   ├── evidence/              ← 截图、日志等证据文件
│   └── skills/
│       └── kanban/
│           └── SKILL.md       ← 看板 Skill 版本
```

## 灵感来源

借鉴了开源项目 [LoopX](https://github.com/huangruiteng/loopx) 的控制面设计理念。