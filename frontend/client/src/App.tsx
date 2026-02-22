import { BrowserRouter, Routes, Route, NavLink } from "react-router-dom"
import QuotePage from "./pages/QuotePage"
import RulesPage from "./pages/RulesPage"
import JobsPage from "./pages/JobsPage"
import BulkQuotePage from "./pages/BulkQuotePage"

export default function App() {
  return (
    <BrowserRouter>
      <div className="min-h-screen bg-gray-100">
        <nav className="bg-white shadow-sm">
          <div className="max-w-5xl mx-auto px-6 py-4 flex gap-6 items-center">
            <span className="font-bold text-blue-600 text-lg">
              Mini Pricing Platform
            </span>

             <NavLink to="/rules"
              className={({ isActive }) =>
                isActive ? "text-blue-600 font-medium" : "text-gray-600 hover:text-blue-600"
              }
            >
              Rules
            </NavLink>
            <NavLink to="/quote" end
              className={({ isActive }) =>
                isActive ? "text-blue-600 font-medium" : "text-gray-600 hover:text-blue-600"
              }
            >
              Quote
            </NavLink>
           
            <NavLink to="/bulk"
              className={({ isActive }) =>
                isActive ? "text-blue-600 font-medium" : "text-gray-600 hover:text-blue-600"
              }
            >
              Bulk Quote
            </NavLink>
            <NavLink to="/jobs"
              className={({ isActive }) =>
                isActive ? "text-blue-600 font-medium" : "text-gray-600 hover:text-blue-600"
              }
            >
              Jobs
            </NavLink>
          </div>
        </nav>
        <main className="max-w-5xl mx-auto px-6 py-8">
          <Routes>
            <Route path="/quote" element={<QuotePage />} />
            <Route path="/rules" element={<RulesPage />} />
            <Route path="/jobs" element={<JobsPage />} />
            <Route path="/bulk" element={<BulkQuotePage />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  )
}