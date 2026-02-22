import { useEffect, useState } from "react"
import { getJob } from "../services/api"
import type { Job } from "../types"
import { JobStatus } from "../types"

export default function JobsPage() {
  const [jobId, setJobId] = useState("")
  const [job, setJob] = useState<Job | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [polling, setPolling] = useState(false)

  const fetchJob = async (id: string) => {
    setLoading(true)
    setError(null)
    try {
      const data = await getJob(id)
      setJob(data)
      return data
    } catch {
      setError("Job not found")
      return null
    } finally {
      setLoading(false)
    }
  }

  
const [history, setHistory] = useState<{
  jobId: string
  submittedAt: string
  totalItems: number
}[]>([])
  const handleSearch = async () => {
    if (!jobId.trim()) return
    await fetchJob(jobId)
  }

  useEffect(() => {
  const saved = JSON.parse(localStorage.getItem("jobHistory") || "[]")
  setHistory(saved)
}, [])

  const handlePoll = async () => {
    if (!jobId.trim()) return
    setPolling(true)

    const interval = setInterval(async () => {
      const data = await fetchJob(jobId)
      if (
        data?.status === JobStatus.Completed ||
        data?.status === JobStatus.Failed ||
        !data
      ) {
        clearInterval(interval)
        setPolling(false)
      }
    }, 1000)
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case JobStatus.Completed: return "bg-green-100 text-green-700"
      case JobStatus.Processing: return "bg-blue-100 text-blue-700"
      case JobStatus.Pending: return "bg-yellow-100 text-yellow-700"
      case JobStatus.Failed: return "bg-red-100 text-red-700"
      default: return "bg-gray-100 text-gray-700"
    }
  }

  const progressPercent = job
    ? Math.round((job.processedItems / job.totalItems) * 100)
    : 0

  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-2xl font-bold text-gray-800 mb-6">Job Tracker</h1>

      {/* Search */}
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Job ID
        </label>
        <div className="flex gap-3">
          <input
            type="text"
            value={jobId}
            onChange={e => setJobId(e.target.value)}
            className="flex-1 border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g. 39721c8a-2f39-4df8-8366-56d966f9169f"
          />
          <button
            onClick={handleSearch}
            disabled={loading}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 font-medium"
          >
            {loading ? "Loading..." : "Search"}
          </button>
          <button
            onClick={handlePoll}
            disabled={polling || !jobId.trim()}
            className="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 disabled:opacity-50 font-medium"
          >
            {polling ? "Polling..." : "Auto Poll"}
          </button>
        </div>
        <p className="text-xs text-gray-500 mt-2">
          Auto Poll จะ refresh ทุก 1 วินาที จนกว่า job จะเสร็จ
        </p>
      </div>

      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 text-red-700 rounded-lg p-4">
          {error}
        </div>
      )}

      {/* Job History */}
{history.length > 0 && (
  <div className="bg-white rounded-lg shadow p-6 mb-6">
    <h2 className="font-medium text-gray-700 mb-3">Recent Jobs</h2>
    <div className="space-y-2">
      {history.map((h, i) => (
        <div
          key={i}
          onClick={() => {
            setJobId(h.jobId)
            fetchJob(h.jobId)
          }}
          className="flex justify-between items-center p-2 hover:bg-gray-50 rounded cursor-pointer"
        >
          <div>
            <p className="text-sm font-mono text-gray-700">{h.jobId}</p>
            <p className="text-xs text-gray-500">{h.submittedAt} · {h.totalItems} quotes</p>
          </div>
          <span className="text-xs text-blue-600">View →</span>
        </div>
      ))}
    </div>
  </div>
)}

      {/* Job Detail */}
      {job && (
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex justify-between items-start mb-4">
            <div>
              <p className="text-xs text-gray-500 mb-1">Job ID</p>
              <p className="font-mono text-sm text-gray-700">{job.id}</p>
            </div>
            <span className={`text-sm px-3 py-1 rounded-full font-medium ${getStatusColor(job.status)}`}>
              {job.status}
            </span>
          </div>

          {/* Progress Bar */}
          <div className="mb-4">
            <div className="flex justify-between text-sm text-gray-600 mb-1">
              <span>Progress</span>
              <span>{job.processedItems} / {job.totalItems} ({progressPercent}%)</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className="bg-blue-600 h-2 rounded-full transition-all"
                style={{ width: `${progressPercent}%` }}
              />
            </div>
          </div>

          {/* Timestamps */}
          <div className="grid grid-cols-2 gap-4 mb-4 text-sm">
            <div>
              <p className="text-gray-500">Created</p>
              <p className="text-gray-700">
                {new Date(job.createdAt).toLocaleString()}
              </p>
            </div>
            {job.completedAt && (
              <div>
                <p className="text-gray-500">Completed</p>
                <p className="text-gray-700">
                  {new Date(job.completedAt).toLocaleString()}
                </p>
              </div>
            )}
          </div>

          {/* Error */}
          {job.errorMessage && (
            <div className="mb-4 bg-red-50 border border-red-200 text-red-700 rounded-lg p-3 text-sm">
              {job.errorMessage}
            </div>
          )}

          {/* Results */}
          {job.results.length > 0 && (
            <div>
              <h3 className="font-medium text-gray-800 mb-3">
                Results ({job.results.length})
              </h3>
              <div className="space-y-2 max-h-96 overflow-y-auto">
                {job.results.map((result, i) => (
                  <div key={i} className="border rounded-lg p-3 text-sm">
                    <div className="flex justify-between items-center mb-1">
                      <span className="text-gray-500">Quote #{i + 1}</span>
                      <div className="flex gap-4">
                        <span className="text-gray-500">
                          Base: ฿{result.basePrice.toFixed(2)}
                        </span>
                        <span className="font-medium text-blue-700">
                          Final: ฿{result.finalPrice.toFixed(2)}
                        </span>
                      </div>
                    </div>
                    {result.appliedRules.length > 0 && (
                      <p className="text-xs text-gray-500">
                        Rules: {result.appliedRules.join(", ")}
                      </p>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}