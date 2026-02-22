import axios from "axios";
import type {
  QuoteRequest,
  BulkQuoteRequest,
  QuoteResult,
  BulkJobResponse,
  Job,
  PricingRule,
  CreateRuleRequest
} from "../types";
const API_URL = import.meta.env.VITE_API_URL 

const api = axios.create({
  baseURL:API_URL,
  headers: { "Content-Type": "application/json" }
});

api.interceptors.response.use(
  (response) => {
    // log Correlation ID จาก response header
    const correlationId = response.headers["x-correlation-id"]
    console.log(`[${response.config.method?.toUpperCase()}] ${response.config.url} → ID: ${correlationId}`)
    return response
  },
  (error) => {
    const correlationId = error.response?.headers["x-correlation-id"]
    console.error(`[ERROR] ID: ${correlationId}`, error.message)
    return Promise.reject(error)
  }
)
api.interceptors.request.use((config) => {
  // generate Correlation ID ทุก request
  const correlationId = crypto.randomUUID()
  config.headers["X-Correlation-ID"] = correlationId
  return config
})
// ─── Quotes ───────────────────────────────────────────
export const calculatePrice = async (request: QuoteRequest): Promise<QuoteResult> => {
  const { data } = await api.post<QuoteResult>("/quotes/price", request);
  return data;
};

export const submitBulk = async (request: BulkQuoteRequest): Promise<BulkJobResponse> => {
  const { data } = await api.post<BulkJobResponse>("/quotes/bulk", request);
  return data;
};

// ─── Jobs ─────────────────────────────────────────────
export const getJob = async (jobId: string): Promise<Job> => {
  const { data } = await api.get<Job>(`/jobs/${jobId}`);
  return data;
};

// ─── Rules ────────────────────────────────────────────
export const getRules = async (): Promise<PricingRule[]> => {
  const { data } = await api.get<PricingRule[]>("/rules");
  return data;
};

export const getRuleById = async (id: string): Promise<PricingRule> => {
  const { data } = await api.get<PricingRule>(`/rules/${id}`);
  return data;
};

export const createRule = async (rule: CreateRuleRequest): Promise<PricingRule> => {
  const { data } = await api.post<PricingRule>("/rules", rule);
  return data;
};

export const updateRule = async (id: string, rule: CreateRuleRequest): Promise<PricingRule> => {
  const { data } = await api.put<PricingRule>(`/rules/${id}`, rule);
  return data;
};

export const deleteRule = async (id: string): Promise<void> => {
  await api.delete(`/rules/${id}`);
};

// ─── Health ───────────────────────────────────────────
export const checkHealth = async () => {
  const { data } = await api.get("/health");
  return data;
};