import { useState } from "react"
import { submitBulk } from "../services/api"
import type { QuoteRequest } from "../types"
import { useNavigate } from "react-router-dom"

const emptyQuote = (): QuoteRequest => ({
  weightKg: 0,
  destinationZipCode: "",
  shipmentDate: new Date().toISOString().slice(0, 16),
  declaredValue: 0
})



export default function BulkQuotePage() {
  const [quotes, setQuotes] = useState<QuoteRequest[]>([emptyQuote()])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [uploadedFileName, setUploadedFileName] = useState<string | null>(null)
  const navigate = useNavigate()

  const [validationError, setValidationError] = useState<string | null>(null)

const validate = () => {
  for (let i = 0; i < quotes.length; i++) {
    const q = quotes[i]
    if (!q.weightKg || q.weightKg <= 0)
      return `Quote #${i + 1}: Weight must be greater than 0`
    if (!q.destinationZipCode.trim())
      return `Quote #${i + 1}: Zip code is required`
    if (!/^\d{5}$/.test(q.destinationZipCode))
      return `Quote #${i + 1}: Zip code must be 5 digits`
  }
  return null
}

  // ─── Upload Handler ───────────────────────────────────
  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file) return

    setError(null)
    setUploadedFileName(file.name)
    const reader = new FileReader()

    reader.onload = (event) => {
      const content = event.target?.result as string
      try {
        if (file.name.endsWith(".json")) {
          parseJSON(content)
        } else if (file.name.endsWith(".csv")) {
          parseCSV(content)
        } else {
          setError("รองรับเฉพาะ .json และ .csv เท่านั้น")
        }
      } catch {
        setError("ไฟล์ไม่ถูกต้อง กรุณาตรวจสอบ format")
      }
    }
    reader.readAsText(file)
  }

  const parseJSON = (content: string) => {
    const parsed = JSON.parse(content)
    // รองรับทั้ง { quotes: [...] } และ [...]
    const data = Array.isArray(parsed) ? parsed : parsed.quotes
    if (!Array.isArray(data)) throw new Error("Invalid format")
    setQuotes(data)
  }

  const parseCSV = (content: string) => {
    const lines = content.trim().split("\n")
    const headers = lines[0].split(",").map(h => h.trim())
    const data = lines.slice(1).map(line => {
      const values = line.split(",").map(v => v.trim())
      const obj: Record<string, string> = {}
      headers.forEach((h, i) => obj[h] = values[i])
      return {
        weightKg: parseFloat(obj.weightKg) || 0,
        destinationZipCode: obj.destinationZipCode || "",
        shipmentDate: obj.shipmentDate || new Date().toISOString().slice(0, 16),
        declaredValue: parseFloat(obj.declaredValue) || 0
      } as QuoteRequest
    })
    setQuotes(data)
  }

  // ─── Form Handlers ────────────────────────────────────
  const addQuote = () => setQuotes([...quotes, emptyQuote()])

  const removeQuote = (index: number) => {
    if (quotes.length === 1) return
    setQuotes(quotes.filter((_, i) => i !== index))
  }

  const updateQuote = (index: number, field: keyof QuoteRequest, value: string | number) => {
    const updated = [...quotes]
    updated[index] = { ...updated[index], [field]: value }
    setQuotes(updated)
  }

const handleSubmit = async () => {
  const validErr = validate()
  if (validErr) {
    setValidationError(validErr)
    return
  }
  setValidationError(null)
  setLoading(true)
  setError(null)
  try {
    const result = await submitBulk({ quotes })
    const history = JSON.parse(localStorage.getItem("jobHistory") || "[]")
    history.unshift({
      jobId: result.job_id,
      submittedAt: new Date().toLocaleString(),
      totalItems: quotes.length
    })
    localStorage.setItem("jobHistory", JSON.stringify(history.slice(0, 10)))
    navigate(`/jobs?jobId=${result.job_id}`)
  } catch {
    setError("Failed to submit bulk job")
  } finally {
    setLoading(false)
  }
}
  return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Bulk Quote</h1>
        <button
          onClick={addQuote}
          className="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 font-medium"
        >
          + Add Quote
        </button>
      </div>

      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 text-red-700 rounded-lg p-4">
          {error}
        </div>
      )}

      {validationError && (
        <div className="mb-4 bg-yellow-50 border border-yellow-200 text-yellow-700 rounded-lg p-4">
          {validationError}
        </div>
      )}
 

      {/* Manual Form */}
      <div className="space-y-4 mb-6">
        {quotes.map((quote, index) => (
          <div key={index} className="bg-white rounded-lg shadow p-4">
            <div className="flex justify-between items-center mb-3">
              <h3 className="font-medium text-gray-700">Quote #{index + 1}</h3>
              <button
                onClick={() => removeQuote(index)}
                disabled={quotes.length === 1}
                className="text-red-500 hover:text-red-700 text-sm disabled:opacity-30"
              >
                Remove
              </button>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">Weight (kg)</label>
                <input
                  type="text"
                  value={quote.weightKg === 0 ? "" : quote.weightKg}
                  onChange={e => updateQuote(index, "weightKg", parseFloat(e.target.value) || 0)}
                  className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="e.g. 15"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">Destination Zip Code</label>
                <input
                  type="text"
                  value={quote.destinationZipCode}
                  onChange={e => updateQuote(index, "destinationZipCode", e.target.value)}
                  className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="e.g. 90210"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">Shipment Date</label>
                <input
                  type="datetime-local"
                  value={quote.shipmentDate}
                  onChange={e => updateQuote(index, "shipmentDate", e.target.value)}
                  className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">Declared Value (฿)</label>
                <input
                  type="text"
                  value={quote.declaredValue === 0 ? "" : quote.declaredValue}
                  onChange={e => updateQuote(index, "declaredValue", parseFloat(e.target.value) || 0)}
                  className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="e.g. 500"
                />
              </div>
            </div>
          </div>
        ))}
      </div>

           {/* Upload Section */}
      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h2 className="font-medium text-gray-700 mb-3">Upload File</h2>
        <label className="flex flex-col items-center justify-center w-full h-32 border-2 border-dashed border-gray-300 rounded-lg cursor-pointer hover:border-blue-400 hover:bg-blue-50 transition-colors">
          <div className="text-center">
            {uploadedFileName ? (
              <>
                <p className="text-green-600 font-medium">✅ {uploadedFileName}</p>
                <p className="text-sm text-gray-500 mt-1">{quotes.length} quotes loaded</p>
              </>
            ) : (
              <>
                <p className="text-gray-500">คลิกเพื่ออัพโหลดไฟล์</p>
                <p className="text-xs text-gray-400 mt-1">รองรับ .json และ .csv</p>
              </>
            )}
          </div>
          <input
            type="file"
            accept=".json,.csv"
            onChange={handleFileUpload}
            className="hidden"
          />
        </label>
      </div>

      <div className="flex justify-between items-center">
        <p className="text-sm text-gray-500">{quotes.length} quote(s)</p>
        <button
          onClick={handleSubmit}
          disabled={loading}
          className="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 font-medium"
        >
          {loading ? "Submitting..." : "Submit Bulk Job"}
        </button>
      </div>
    </div>
  )
}

