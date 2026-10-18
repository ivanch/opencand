import React, { Suspense, lazy, useEffect } from 'react';
import { Routes, Route, useLocation } from 'react-router-dom';
import Navbar from './components/Navbar';
import HeroSection from './components/HeroSection';
import StatisticsSection from './components/StatisticsSection';
import FeaturesSection from './components/FeaturesSection';
import Footer from './components/Footer';
import NotFound from './components/NotFound';
import MatrixBackground from './components/MatrixBackground';
import ConsentBanner from './components/ConsentBanner';
import './App.css';

// Route-level code splitting: recharts and the secondary pages stay out of the main bundle
// (CandidatePage is split too: it is the only importer of recharts)
const CandidatePage = lazy(() => import('./components/CandidatePage/CandidatePage'));
const DataStatsPage = lazy(() => import('./components/DataStatsPage'));
const StatisticsPage = lazy(() => import('./components/StatisticsPage/StatisticsPage'));
const SobrePage = lazy(() => import('./components/SobrePage'));

const RouteFallback: React.FC = () => (
  <div className="flex items-center justify-center py-32" role="status" aria-label="Carregando">
    <div className="animate-spin rounded-full h-10 w-10 border-4 border-indigo-400 border-t-transparent"></div>
  </div>
);

// HomePage component
const HomePage: React.FC = () => {
  return (
    <main className="flex-grow">
      <HeroSection />
      <div className="w-32 h-1 bg-gradient-to-r from-indigo-400 to-purple-400 mx-auto mt-12 rounded-full"></div>
      <StatisticsSection />
      <div className="w-32 h-1 bg-gradient-to-r from-indigo-400 to-purple-400 mx-auto mt-6 rounded-full"></div>
      <FeaturesSection />
    </main>
  );
};

function App() {
  const location = useLocation();

  useEffect(() => {
    const segment = location.hash;
    if (segment) {
      const element = document.getElementById(segment.replace('#', ''));
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    }
  }, [location]);

  return (
    <div className="min-h-screen w-full flex flex-col relative" style={{ backgroundColor: 'transparent' }}>
      <MatrixBackground />
      <Navbar />
      <div className="relative z-10 flex-1 flex flex-col">
        <div className="flex-1">
          <Suspense fallback={<RouteFallback />}>
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/candidato/:id" element={<CandidatePage />} />
              <Route path="/dados-disponiveis" element={<DataStatsPage />} />
              <Route path="/estatisticas" element={<StatisticsPage />} />
              <Route path="/sobre" element={<SobrePage />} />
              <Route path="*" element={<NotFound />} />
            </Routes>
          </Suspense>
        </div>
        <Footer />
      </div>
      <ConsentBanner />
    </div>
  );
}

export default App;
