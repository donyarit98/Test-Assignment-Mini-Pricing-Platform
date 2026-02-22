export const RuleType = {
  TimeWindowPromotion: 0,
  RemoteAreaSurcharge: 1,
  WeightTier: 2
} as const
export type RuleType = typeof RuleType[keyof typeof RuleType]

export const JobStatus = {
  Pending: "Pending",
  Processing: "Processing",
  Completed: "Completed",
  Failed: "Failed"
} as const

export type JobStatus = typeof JobStatus[keyof typeof JobStatus]
// ─── Requests ─────────────────────────────────────────
export interface QuoteRequest {
  weightKg: number
  destinationZipCode: string
  shipmentDate: string
  declaredValue: number
}

export interface BulkQuoteRequest {
  quotes: QuoteRequest[]
}

export interface CreateRuleRequest {
  name: string
  type: RuleType
  priority: number
  effectiveFrom: string
  effectiveTo: string
  isActive: boolean
  windowStart?: string
  windowEnd?: string
  discountPercent?: number
  remoteZipCodes?: string[]
  surchargeAmount?: number
  weightFrom?: number
  weightTo?: number
  pricePerKg?: number
}

// ─── Responses ────────────────────────────────────────
export interface QuoteResult {
  quoteId: string
  basePrice: number
  finalPrice: number
  appliedRules: string[]
  error?: string
}

export interface BulkJobResponse {
  job_id: string
  message: string
}

export interface Job {
  id: string
  status: JobStatus
  createdAt: string
  completedAt?: string
  totalItems: number
  processedItems: number
  results: QuoteResult[]
  errorMessage?: string
}

export interface PricingRule {
  id: string
  name: string
  type: RuleType
  priority: number
  effectiveFrom: string
  effectiveTo: string
  isActive: boolean
  windowStart?: string
  windowEnd?: string
  discountPercent?: number
  remoteZipCodes?: string[]
  surchargeAmount?: number
  weightFrom?: number
  weightTo?: number
  pricePerKg?: number
}