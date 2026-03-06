"use client";

import { useState } from "react";
import { askAI } from "@/lib/api";

export default function Home() {
  const [question, setQuestion] = useState("");
  const [answer, setAnswer] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleAsk() {
    if (!question.trim()) return;
    
    setLoading(true);
    setError("");
    setAnswer("");
    
    const result = await askAI(question);
    
    setLoading(false);
    
    if (result.success) {
      setAnswer(result.answer);
    } else {
      setError(result.error || "An error occurred");
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-zinc-50 to-zinc-100 dark:from-black dark:to-zinc-900">
      <div className="mx-auto max-w-4xl px-6 py-16">
        <header className="mb-12 text-center">
          <h1 className="text-4xl font-bold tracking-tight text-black dark:text-white sm:text-5xl">
            .NET AI Agent Demo
          </h1>
          <p className="mt-4 text-lg text-zinc-600 dark:text-zinc-400">
            Clean Architecture + Semantic Kernel + Next.js
          </p>
        </header>

        <main className="rounded-2xl bg-white p-8 shadow-xl dark:bg-zinc-900 dark:shadow-2xl">
          <div className="flex flex-col gap-4 sm:flex-row">
            <input
              className="flex-1 rounded-lg border border-zinc-300 bg-zinc-50 px-4 py-3 text-black dark:border-zinc-700 dark:bg-zinc-800 dark:text-white focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleAsk()}
              placeholder="Ask something to the AI agent..."
              disabled={loading}
            />
            <button
              className="rounded-lg bg-blue-600 px-6 py-3 font-semibold text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
              onClick={handleAsk}
              disabled={loading || !question.trim()}
            >
              {loading ? "Thinking..." : "Ask"}
            </button>
          </div>

          {error && (
            <div className="mt-6 rounded-lg bg-red-50 p-4 text-red-600 dark:bg-red-900/20 dark:text-red-400">
              {error}
            </div>
          )}

          {answer && (
            <div className="mt-6">
              <h3 className="mb-2 text-sm font-medium text-zinc-500 dark:text-zinc-400">
                Answer:
              </h3>
              <div className="rounded-lg bg-zinc-50 p-4 text-zinc-800 dark:bg-zinc-800 dark:text-zinc-200">
                {answer}
              </div>
            </div>
          )}
        </main>

        <footer className="mt-12 text-center text-sm text-zinc-500 dark:text-zinc-400">
          <p>
            Backend: ASP.NET Core Minimal API • EF Core • PostgreSQL • Redis • Semantic Kernel
          </p>
        </footer>
      </div>
    </div>
  );
}
