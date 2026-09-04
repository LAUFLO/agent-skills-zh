/**
 * Kanban helpers - P0 最小改动实现
 * 1) lessons category  2) frontier 语义去重  3) version 原子写
 */

// --- 1. lessons category ---
export const LESSON_CATEGORIES = ["engineering", "procedure", "preference", "repo"];
export const LESSON_TYPES = ["replan", "self_repair"];

/** 归一化 lessons，旧数据缺 category 视为 engineering */
export function normalizeLesson(lesson) {
  const cat = LESSON_CATEGORIES.includes(lesson.category) ? lesson.category : "engineering";
  return { category: cat, type: lesson.type || "self_repair", content: lesson.content || "", createdAt: lesson.createdAt || new Date().toISOString().slice(0, 10), ...lesson, category: cat };
}

// --- 2. frontier 语义去重 ---
/** 内容归一化：去空格/标点/转小写，用于 hash 去重 */
export function normalizeContent(content) {
  return (content || "").toLowerCase().replace(/[\s\p{P}]/gu, "").trim();
}
export function contentHash(content) {
  const n = normalizeContent(content);
  let h = 5381;
  for (let i = 0; i < n.length; i++) h = ((h << 5) + h) ^ n.charCodeAt(i);
  return (h >>> 0).toString(16);
}
/** 按 hash 去重：已存在则更新 status/evidence，不追加 */
export function upsertTodo(frontier, todo) {
  const hash = contentHash(todo.content);
  const existing = frontier.find((t) => contentHash(t.content) === hash || t.id === todo.id);
  if (existing) {
    Object.assign(existing, { ...todo, id: existing.id });
    return { updated: true, id: existing.id, hash };
  }
  frontier.push(todo);
  return { updated: false, id: todo.id, hash };
}

// --- 3. version 原子写 ---
export function ensureVersion(state) {
  if (typeof state.version !== "number") state.version = 1;
  return state.version;
}
export function checkVersionConflict(prevVersion, currentOnDisk) {
  if (currentOnDisk.version !== prevVersion) {
    throw new Error(`state.json 版本冲突: 内存 v${prevVersion} vs 磁盘 v${currentOnDisk.version}，请重跑 /handover 合并`);
  }
}
export function bumpVersion(state) {
  state.version = (state.version || 1) + 1;
  state.updatedAt = new Date().toISOString();
  return state.version;
}

// --- 4. provider 解耦（万能插座）---
/** 解析输入源：声明的输入源仅为可选输入，无声明则跳过不报错 */
export function resolveProvider(explicit) {
  if (explicit && explicit !== "auto") return explicit;
  return "auto"; // auto = 有声明的输入源文件就读，无则跳过
}
export function shouldReadInputSource(provider, hasInputFiles) {
  if (provider === "none") return false;
  if (provider && provider !== "auto") return true;
  return hasInputFiles; // auto 模式：有文件才读
}

// --- boot-packet 上下文过滤（P0-增强）---
/** 按 frontier 关键词过滤 lessons，比固定最近3条更准 */
export function filterLessonsForBoot(lessons, frontier, limit = 3) {
  const normalized = lessons.map(normalizeLesson);
  if (!frontier.length) return normalized.slice(-limit);
  const keywords = frontier.flatMap((t) => normalizeContent(t.content).slice(0, 20));
  const scored = normalized.map((l) => {
    const c = normalizeContent(l.content);
    const score = keywords.reduce((s, k) => s + (k && c.includes(k.slice(0, 8)) ? 1 : 0), 0);
    const catBoost = l.category === "procedure" || l.category === "engineering" ? 0.5 : 0;
    return { lesson: l, score: score + catBoost };
  });
  scored.sort((a, b) => b.score - a.score);
  const picked = scored.filter((s) => s.score > 0).slice(0, limit).map((s) => s.lesson);
  return picked.length ? picked : normalized.slice(-limit);
}
