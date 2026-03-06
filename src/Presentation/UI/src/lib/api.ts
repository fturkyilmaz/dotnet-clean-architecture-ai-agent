const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export interface AIRequest {
  question: string;
  sessionId?: string;
}

export interface AIResponse {
  answer: string;
  success: boolean;
  error?: string;
  sessionId?: string;
}

export async function askAI(question: string, sessionId?: string): Promise<AIResponse> {
  try {
    const url = new URL(`${API_BASE_URL}/ai/ask`);
    url.searchParams.set('question', question);
    if (sessionId) {
      url.searchParams.set('sessionId', sessionId);
    }

    const response = await fetch(url.toString(), {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      return {
        answer: '',
        success: false,
        error: `API error: ${response.status} - ${response.statusText}`,
      };
    }

    const data = await response.json();
    return {
      answer: data.answer || data.message || JSON.stringify(data),
      success: true,
      sessionId: data.sessionId,
    };
  } catch (error) {
    return {
      answer: '',
      success: false,
      error: error instanceof Error ? error.message : 'Unknown error occurred',
    };
  }
}

export async function askAIPost(question: string, sessionId?: string): Promise<AIResponse> {
  try {
    const response = await fetch(`${API_BASE_URL}/ai/ask`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ question, sessionId }),
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
      sessionId: data.sessionId,
    };
  } catch (error) {
    return {
      answer: '',
      success: false,
      error: error instanceof Error ? error.message : 'Unknown error occurred',
    };
  }
}
