"use client";

import { useState, useCallback } from "react";
import { askAI, AIResponse } from "@/lib/api";

export default function Home() {
  const [question, setQuestion] = useState("");
  const [answer, setAnswer] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [sessionId] = useState(() => crypto.randomUUID());

  const handleAsk = useCallback(async () => {
    if (!question.trim()) return;
    
    setLoading(true);
    setError("");
    setAnswer("");
    
    try {
      const result: AIResponse = await askAI(question, sessionId);
      
      if (result.success) {
        setAnswer(result.answer);
      } else {
        setError(result.error || "An error occurred");
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "An unexpected error occurred");
    } finally {
      setLoading(false);
    }
  }, [question, sessionId]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-zinc-50 to-zinc-100 dark:from-black dark:to-zinc-900">
      <div className="mx-auto max-w-4xl px-6 py-16">
        <header className="mb-12 text-center">
          <h1 className="text-4xl font-bold tracking-tight text-black dark:text-white sm:text-5xl">
            .NET AI Agent
          </h1>
          <p className="mt-4 text-lg text-zinc-600 dark:text-zinc-400">
            Semantic Kernel • Clean Architecture • Microservices
          </p>
        </header>

        <main className="rounded-2xl bg-white p-8 shadow-xl dark:bg-zinc-900 dark:shadow-2xl">
          <div className="flex flex-col gap-4 sm:flex-row">
            <input
              className="flex-1 rounded-lg border border-zinc-300 bg-zinc-50 px-4 py-3 text-black dark:border-zinc-700 dark:bg-zinc-800 dark:text-white focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleAsk()}
              placeholder="Ask the AI agent..."
              disabled={loading}
            />
            <button
              className="rounded-lg bg-blue-600 px-6 py-3 font-semibold text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
              onClick={handleAsk}
              disabled={loading || !question.trim()}
            >
              {loading ? (
                <span className="flex items-center gap-2">
                  <svg className="h-4 w-4 animate-spin" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                  </svg>
                  Processing...
                </span>
              ) : (
                "Ask"
              )}
            </button>
          </div>

          {error && (
            <div className="mt-6 flex items-center gap-3 rounded-lg border border-red-200 bg-red-50 p-4 dark:border-red-800 dark:bg-red-900/20">
              <svg className="h-5 w-5 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p className="text-red-600 dark:text-red-400">{error}</p>
            </div>
          )}

          {answer && (
            <div className="mt-6">
              <h3 className="mb-2 text-sm font-medium text-zinc-500 dark:text-zinc-400">
                Response:
              </h3>
              <div className="prose prose-zinc dark:prose-invert max-w-none rounded-lg bg-zinc-50 p-4 dark:bg-zinc-800">
                <p className="whitespace-pre-wrap text-zinc-800 dark:text-zinc-200">{answer}</p>
              </div>
            </div>
          )}
        </main>

        <footer className="mt-12 text-center text-sm text-zinc-500 dark:text-zinc-400">
          <div className="flex justify-center gap-4">
            <span className="flex items-center gap-1">
              <span className="h-2 w-2 rounded-full bg-green-500"></span>
              API: :5000
            </span>
            <span className="flex items-center gap-1">
              <span className="h-2 w-2 rounded-full bg-blue-500"></span>
              UI: :3000
            </span>
          </div>
        </footer>
      </div>
    </div>
  );
}
