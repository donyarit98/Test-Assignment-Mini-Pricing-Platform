import { useState, useEffect } from "react"
import { getRules, createRule, updateRule, deleteRule } from "../services/api"
import type { PricingRule, CreateRuleRequest } from "../types"
import { RuleType } from "../types"

const emptyForm: CreateRuleRequest = {
  name: "",
  type: RuleType.WeightTier,
  priority: 1,
  effectiveFrom: "2024-01-01T00:00:00",
  effectiveTo: "2099-12-31T00:00:00",
  isActive: true,
  discountPercent: undefined,
  remoteZipCodes: [],
  surchargeAmount: undefined,
  weightFrom: undefined,
  weightTo: undefined,
  pricePerKg: undefined,
  windowStart: undefined,
  windowEnd: undefined
}

export default function RulesPage() {
  const [rules, setRules] = useState<PricingRule[]>([])
  const [loading, setLoading] = useState(false)
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [form, setForm] = useState<CreateRuleRequest>(emptyForm)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchRules()
  }, [])

  const fetchRules = async () => {
    setLoading(true)
    try {
      const data = await getRules()
      setRules(data)
    } catch {
      setError("Failed to load rules")
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async () => {
    try {
      if (editingId) {
        await updateRule(editingId, form)
      } else {
        await createRule(form)
      }
      setShowForm(false)
      setEditingId(null)
      setForm(emptyForm)
      fetchRules()
    } catch {
      setError("Failed to save rule")
    }
  }

  const handleEdit = (rule: PricingRule) => {
    setForm({
      name: rule.name,
      type: rule.type,
      priority: rule.priority,
      effectiveFrom: rule.effectiveFrom,
      effectiveTo: rule.effectiveTo,
      isActive: rule.isActive,
      discountPercent: rule.discountPercent,
      remoteZipCodes: rule.remoteZipCodes,
      surchargeAmount: rule.surchargeAmount,
      weightFrom: rule.weightFrom,
      weightTo: rule.weightTo,
      pricePerKg: rule.pricePerKg,
      windowStart: rule.windowStart,
      windowEnd: rule.windowEnd
    })
    setEditingId(rule.id)
    setShowForm(true)
  }

  const handleDelete = async (id: string) => {
    if (!confirm("Delete this rule?")) return
    try {
      await deleteRule(id)
      fetchRules()
    } catch {
      setError("Failed to delete rule")
    }
  }
  return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Pricing Rules</h1>
        <button
          onClick={() => { setShowForm(true); setEditingId(null); setForm(emptyForm) }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 font-medium"
        >
          + Add Rule
        </button>
      </div>

      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 text-red-700 rounded-lg p-4">
          {error}
        </div>
      )}

      {/* Form Modal */}
      {showForm && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl p-6 w-full max-w-lg max-h-screen overflow-y-auto">
            <h2 className="text-lg font-bold mb-4">
              {editingId ? "Edit Rule" : "Create Rule"}
            </h2>

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
                <input
                  type="text"
                  value={form.name}
                  onChange={e => setForm({ ...form, name: e.target.value })}
                  className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="Rule name"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                <select
                  value={form.type}
                  onChange={e => setForm({ ...form, type: +e.target.value as RuleType })}
                  className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value={0}>Time Window Promotion</option>
                  <option value={1}>Remote Area Surcharge</option>
                  <option value={2}>Weight Tier</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Priority</label>
                <input
                  type="number"
                  value={form.priority}
                  onChange={e => setForm({ ...form, priority: +e.target.value })}
                  className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={e => setForm({ ...form, isActive: e.target.checked })}
                  className="w-4 h-4"
                />
                <label className="text-sm font-medium text-gray-700">Active</label>
              </div>

              {/* TimeWindow fields */}
              {form.type === 0 && (
                <>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Window Start (HH:MM:SS)
                    </label>
                    <input
                      type="text"
                      value={form.windowStart ?? ""}
                      onChange={e => setForm({ ...form, windowStart: e.target.value })}
                      className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="06:00:00"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Window End (HH:MM:SS)
                    </label>
                    <input
                      type="text"
                      value={form.windowEnd ?? ""}
                      onChange={e => setForm({ ...form, windowEnd: e.target.value })}
                      className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="09:00:00"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Discount (%)
                    </label>
                    <input
                      type="number"
                      value={form.discountPercent ?? ""}
                      onChange={e => setForm({ ...form, discountPercent: +e.target.value })}
                      className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="10"
                    />
                  </div>
                </>
              )}

              {/* RemoteArea fields */}
              {form.type === 1 && (
                <>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Remote Zip Codes (comma separated)
                    </label>
                    <input
                      type="text"
                      value={form.remoteZipCodes?.join(",") ?? ""}
                      onChange={e => setForm({
                        ...form,
                        remoteZipCodes: e.target.value.split(",").map(z => z.trim())
                      })}
                      className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="90210, 10001, 77001"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Surcharge Amount (฿)
                    </label>
                    <input
                      type="number"
                      value={form.surchargeAmount ?? ""}
                      onChange={e => setForm({ ...form, surchargeAmount: +e.target.value })}
                      className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="50"
                    />
                  </div>
                </>
              )}

              {/* WeightTier fields */}
              {form.type === 2 && (
                <>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Weight From (kg)
                      </label>
                      <input
                        type="number"
                        value={form.weightFrom ?? ""}
                        onChange={e => setForm({ ...form, weightFrom: +e.target.value })}
                        className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="10"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">
                        Weight To (kg)
                      </label>
                      <input
                        type="number"
                        value={form.weightTo ?? ""}
                        onChange={e => setForm({ ...form, weightTo: +e.target.value })}
                        className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="50"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Price Per Kg (฿)
                    </label>
                    <input
                      type="number"
                      value={form.pricePerKg ?? ""}
                      onChange={e => setForm({ ...form, pricePerKg: +e.target.value })}
                      className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="15"
                    />
                  </div>
                </>
              )}
            </div>

            <div className="flex gap-3 mt-6">
              <button
                onClick={handleSubmit}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 font-medium"
              >
                {editingId ? "Update" : "Create"}
              </button>
              <button
                onClick={() => { setShowForm(false); setEditingId(null) }}
                className="flex-1 bg-gray-100 text-gray-700 py-2 rounded-lg hover:bg-gray-200 font-medium"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Rules List */}
      {loading ? (
        <p className="text-gray-500">Loading...</p>
      ) : (
        <div className="space-y-3">
          {rules.map(rule => (
            <div key={rule.id} className="bg-white rounded-lg shadow p-4">
              <div className="flex justify-between items-start">
                <div className="flex items-center gap-3">
          
                  <h3 className="font-medium text-gray-800">{rule.name}</h3>
                  <span className="text-xs text-gray-500">Priority: {rule.priority}</span>
                  <span className={`text-xs px-2 py-1 rounded-full ${rule.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                    {rule.isActive ? "Active" : "Inactive"}
                  </span>
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={() => handleEdit(rule)}
                    className="text-blue-600 hover:text-blue-800 text-sm font-medium"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleDelete(rule.id)}
                    className="text-red-600 hover:text-red-800 text-sm font-medium"
                  >
                    Delete
                  </button>
                </div>
              </div>
            </div>
          ))}
          {rules.length === 0 && (
            <p className="text-gray-500 text-center py-8">No rules found</p>
          )}
        </div>
      )}
    </div>
  )
}