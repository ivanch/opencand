import React, { Suspense, lazy, useEffect } from 'react';
import { Routes, Route, useLocation } from 'react-router-dom';
import Navbar from './components/Navbar';
import HeroSection from './components/HeroSection';
import StatisticsSection from './components/StatisticsSection';
import FeaturesSection from './components/FeaturesSection';
import Footer from './components/Footer';
import NotFound from './components/NotFound';
import AppBackground from './components/AppBackground';
import ConsentBanner from './components/ConsentBanner';
import { LoadingBlock } from './shared/Spinner';
import { usePageTitle } from './hooks/usePageTitle';
import './App.css';

// Route-level code splitting: recharts and the secondary pages stay out of the main bundle
// (CandidatePage is split too: it is the only importer of recharts)
const CandidatePage = lazy(() => import('./components/CandidatePage/CandidatePage'));
const DataStatsPage = lazy(() => import('./components/DataStatsPage'));
const StatisticsPage = lazy(() => import('./components/StatisticsPage/StatisticsPage'));
const SobrePage = lazy(() => import('./components/SobrePage'));

const RouteFallback: React.FC = () => (
  <LoadingBlock heightClassName="py-32" colorClassName="border-indigo-400" />
);

// HomePage component
const HomePage: React.FC = () => {
  usePageTitle(null);

  return (
    <main className="flex-grow">
      <HeroSection />
      <StatisticsSection />
      <FeaturesSection />
    </main>
  );
};

function App() {
  const location = useLocation();

  // Hash links scroll to their section; any other navigation starts at the top of the new page
  useEffect(() => {
    const segment = location.hash;
    if (segment) {
      const element = document.getElementById(segment.replace('#', ''));
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
    } else {
      window.scrollTo({ top: 0 });
    }
  }, [location.pathname, location.hash]);

  return (
    <div className="relative isolate flex min-h-screen w-full flex-col">
      <AppBackground />
      <Navbar />
      <div className="relative z-10 flex flex-1 flex-col">
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
