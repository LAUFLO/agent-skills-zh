# LAUFLO — 项目看板

## 项目概览
聚合目录 `D:\LAUFLO`，当前以 `praxis-ouonet-v2` 为主子项目。看板真相源 `.opencode/handover/state.json`。

## 项目结构
```
LAUFLO/
  .opencode/handover/  state.json / boot-packet.md / project_summary.md / evidence/ / lib.mjs
  AGENTS.md          新会话入口与自动看板规则
  praxis-ouonet-v2/  主项目（独立 git）
  scripts/           github-poll 等
```

## 当前阶段进度 (phase-1 / P0)
- **goal**: 落地 3 项最小改动：lessons 分类、frontier 语义去重、version 原子写
- **frontier**: 无（待吸收）
- **gates**: 无

## 已完成阶段
- 初始化看板基座：`lib.mjs` + `state.json` + `boot-packet` + `AGENTS.md`

## 设计规范/约定
- 单一事实源 `state.json`，Praxis 工件为输入源，不可反向修改
- 缺省兼容：旧 lessons 无 category 视为 engineering，旧 state 无 version 视为 

## 后续注意事项
- `ship` 前必须先 `/handover` 吸收，避免 staging 删除丢失
- 多 worktree 并发写需校验 version，冲突时重跑

## Staging spec 决策摘要
暂无 `docs/staging/specs/*.md`
