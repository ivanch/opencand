import React, { useEffect } from 'react';
import { Routes, Route, useLocation } from 'react-router-dom';
import Navbar from './components/Navbar';
import HeroSection from './components/HeroSection';
import StatisticsSection from './components/StatisticsSection';
import FeaturesSection from './components/FeaturesSection';
import Footer from './components/Footer';
import CandidatePage from './components/CandidatePage/CandidatePage';
import DataStatsPage from './components/DataStatsPage';
import StatisticsPage from './components/StatisticsPage';
import SobrePage from './components/SobrePage';
import NotFound from './components/NotFound';
import MatrixBackground from './components/MatrixBackground';
import './App.css';

// HomePage component
const HomePage: React.FC = () => {
  const location = useLocation();

  useEffect(() => {
    if (location.hash) {
      const element = document.getElementById(location.hash.substring(1));
      if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }, [location]);

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
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/candidato/:id" element={<CandidatePage />} />
            <Route path="/dados-disponiveis" element={<DataStatsPage />} />
            <Route path="/estatisticas" element={<StatisticsPage />} />
            <Route path="/sobre" element={<SobrePage />} />
            <Route path="*" element={<NotFound />} />
          </Routes>
        </div>
        <Footer />
      </div>
    </div>
  );
}

export default App;
