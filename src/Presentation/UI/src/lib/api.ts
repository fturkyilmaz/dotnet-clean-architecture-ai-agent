const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export interface AIRequest {
  question: string;
}

export interface AIResponse {
  answer: string;
  success: boolean;
  error?: string;
}

export async function askAI(question: string): Promise<AIResponse> {
  try {
    const response = await fetch(`${API_BASE_URL}/ai/ask?question=${encodeURIComponent(question)}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      return {
        answer: '',
        success: false,
        error: `API error: ${response.status}`,
      };
    }

    const data = await response.json();
    return {
      answer: data.answer || data.message || JSON.stringify(data),
      success: true,
    };
  } catch (error) {
    return {
      answer: '',
      success: false,
      error: error instanceof Error ? error.message : 'Unknown error occurred',
    };
  }
}
