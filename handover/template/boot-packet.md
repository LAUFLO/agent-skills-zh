# Boot Packet — LAUFLO

> 新会话启动时，请首先读取本文件获取当前任务状态。

**当前阶段**: P0 看板最小改动闭环 (phase-1 / active) — 落地 memorax 启发的 3 项最小改动

**可直接推进的 todo (frontier)**: 无，等待 /handover 吸收 Praxis staging

**阻塞 (gates)**: 无

**上次完成摘要**: 初始化 `.opencode/handover/lib.mjs`（含 normalizeLesson/contentHash/upsertTodo/version 校验/filterLessonsForBoot），`state.json` 已升级至 

**关键教训 (按 category 过滤)**:
- [engineering] state.json 增加 version，写入前校验，fail-closed 防并发覆盖

**当前 Praxis staging**: 暂无 `docs/staging/plans/*.md`，以 frontier 为准

**设计约定**: 见 `AGENTS.md` 与 `docs/tech-spec.md`
