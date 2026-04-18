/**
 * @license
 * SPDX-License-Identifier: Apache-2.0
 */

import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout';
import Login from './screens/Login';
import Dashboard from './screens/Dashboard';
import UserManagement from './screens/UserManagement';
import ResearchArea from './screens/ResearchArea';
import Allocation from './screens/Allocation';
import Analytics from './screens/Analytics';

export default function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Login />} />
        
        <Route
          path="/dashboard"
          element={
            <Layout title="System Overview">
              <Dashboard />
            </Layout>
          }
        />
        
        <Route
          path="/research"
          element={
            <Layout title="Research Vectors">
              <ResearchArea />
            </Layout>
          }
        />
        
        <Route
          path="/users"
          element={
            <Layout title="Personnel Management">
              <UserManagement />
            </Layout>
          }
        />

        <Route
          path="/allocation"
          element={
            <Layout title="Allocation Matrix">
              <Allocation />
            </Layout>
          }
        />

        <Route
          path="/analytics"
          element={
            <Layout title="Analytics Core">
              <Analytics />
            </Layout>
          }
        />

        {/* Fallback */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Router>
  );
}
