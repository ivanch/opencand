import React from 'react';
import { Link } from 'react-router-dom';
import GradientButton from '../shared/GradientButton';

interface ErrorPageProps {
  title: string;
  description: string;
  helperText?: string;
}

const ErrorPage: React.FC<ErrorPageProps> = ({ title, description, helperText }) => {
  return (
    <div className="min-h-screen flex items-center justify-center px-4">
      <div className="text-center">
        <div className="mb-8">
          <h1 className="text-9xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-indigo-400 to-purple-400 mb-4">
            {title}
          </h1>
          <p className="text-gray-300 text-lg mb-8 max-w-md mx-auto">
            {description}
          </p>
        </div>
        
        <div className="space-y-4">
          <GradientButton to="/" className="inline-block px-8 py-3">
            Voltar para a página inicial
          </GradientButton>
          
          {helperText && (
            <div className="mt-6">
              <p className="text-gray-400 text-sm">
                {helperText}
              </p>
              <div className="flex justify-center space-x-4 mt-3">
                <Link
                  to="/"
                  className="text-indigo-400 hover:text-indigo-300 transition-colors duration-200"
                >
                  Início
                </Link>
                <span className="text-gray-500">•</span>
                <Link
                  to="/dados-disponiveis"
                  className="text-indigo-400 hover:text-indigo-300 transition-colors duration-200"
                >
                  Dados Disponíveis
                </Link>
              </div>
            </div>
          )}
        </div>
        
        {/* Decorative elements */}
        <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 -z-10">
          <div className="w-96 h-96 bg-gradient-to-r from-indigo-500/10 to-purple-500/10 rounded-full blur-3xl"></div>
        </div>
      </div>
    </div>
  );
};

export default ErrorPage;
