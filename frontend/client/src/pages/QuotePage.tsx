import { useState } from "react"
import { calculatePrice } from "../services/api"
import type { QuoteRequest, QuoteResult } from "../types"

export default function QuotePage() {
  const [form, setForm] = useState<QuoteRequest>({
    weightKg: 0,
    destinationZipCode: "",
    shipmentDate: new Date().toISOString().slice(0, 16),
    declaredValue: 0
  })
  const [result, setResult] = useState<QuoteResult | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async () => {
    setLoading(true)
    setError(null)
    setResult(null)
    try {
      const data = await calculatePrice(form)
      setResult(data)
    } catch (err: unknown) {
        const axiosError = err as { response?: { data?: string } }
        setError(axiosError.response?.data || "Something went wrong")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="max-w-2xl mx-auto p-6">
      <h1 className="text-2xl font-bold text-gray-800 mb-6">
        Calculate Price
      </h1>

      {/* Form */}
      <div className="bg-white rounded-lg shadow p-6 space-y-4">

       <div>
  <label className="block text-sm font-medium text-gray-700 mb-1">
    Weight (kg)
  </label>
  <input
    type="text"
    value={form.weightKg === 0 ? "" : form.weightKg}
    onChange={e => {
      const val = e.target.value
      setForm({ ...form, weightKg: val === "" ? 0 : parseFloat(val) || 0 })
    }}
    className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
    placeholder="e.g. 15"
  />
</div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Destination Zip Code
          </label>
          <input
            type="text"
            value={form.destinationZipCode}
            onChange={e => setForm({ ...form, destinationZipCode: e.target.value })}
            className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g. 90210"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Shipment Date
          </label>
          <input
            type="datetime-local"
            value={form.shipmentDate}
            onChange={e => setForm({ ...form, shipmentDate: e.target.value })}
            className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Declared Value (฿)
          </label>
          <input
            type="number"
            value={form.declaredValue}
            onChange={e => setForm({ ...form, declaredValue: +e.target.value })}
            className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g. 500"
          />
        </div>

        <button
          onClick={handleSubmit}
          disabled={loading}
          className="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 font-medium"
        >
          {loading ? "Calculating..." : "Calculate Price"}
        </button>
      </div>

      {/* Error */}
      {error && (
        <div className="mt-4 bg-red-50 border border-red-200 text-red-700 rounded-lg p-4">
          {error}
        </div>
      )}

      {/* Result */}
      {result && (
        <div className="mt-6 bg-white rounded-lg shadow p-6">
          <h2 className="text-lg font-bold text-gray-800 mb-4">Result</h2>

          <div className="grid grid-cols-2 gap-4 mb-4">
            <div className="bg-gray-50 rounded-lg p-4">
              <p className="text-sm text-gray-500">Base Price</p>
              <p className="text-2xl font-bold text-gray-800">
                ฿{result.basePrice.toFixed(2)}
              </p>
            </div>
            <div className="bg-blue-50 rounded-lg p-4">
              <p className="text-sm text-blue-500">Final Price</p>
              <p className="text-2xl font-bold text-blue-700">
                ฿{result.finalPrice.toFixed(2)}
              </p>
            </div>
          </div>

          {result.appliedRules.length > 0 && (
            <div>
              <p className="text-sm font-medium text-gray-700 mb-2">
                Applied Rules:
              </p>
              <ul className="space-y-1">
                {result.appliedRules.map((rule, i) => (
                  <li key={i} className="flex items-center gap-2 text-sm text-gray-600">
                    <span className="w-2 h-2 bg-green-500 rounded-full"></span>
                    {rule}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {result.appliedRules.length === 0 && (
            <p className="text-sm text-gray-500">No rules applied</p>
          )}
        </div>
      )}
    </div>
  )
}