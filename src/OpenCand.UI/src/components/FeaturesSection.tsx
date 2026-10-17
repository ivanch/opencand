import React from 'react';
import { ArrowDownOnSquareStackIcon, BookOpenIcon, ChartBarIcon, ChartBarSquareIcon, DocumentMagnifyingGlassIcon, DocumentTextIcon, IdentificationIcon, LightBulbIcon, MagnifyingGlassIcon } from '@heroicons/react/24/outline';
import Card from '../shared/Card';

const FeatureCard: React.FC<{ icon: React.ElementType, title: string, children: React.ReactNode }> = ({ icon: Icon, title, children }) => {
  return (
    <Card>
      <Icon className="h-10 w-10 text-indigo-400 mb-4 mx-auto" />
      <h3 className="text-xl font-semibold text-white mb-2">{title}</h3>
      <p className="text-gray-400">{children}</p>
    </Card>
  );
};

const FeaturesSection: React.FC = () => {
  return (
    <section id="recursos" className="py-20">
      <div className="container mx-auto px-4">
        <h2 className="text-4xl md:text-5xl font-bold text-center text-white mb-12">
          Recursos
        </h2>
        <div className="flex flex-wrap justify-center gap-8">
          <FeatureCard icon={DocumentTextIcon} title="Acesso Simplificado">
            Navegue facilmente por dados complexos do TSE com uma interface limpa e amigável.
          </FeatureCard>
          <FeatureCard icon={IdentificationIcon} title="Visualizações Claras">
            Visualização detalhada de perfis, redes sociais e histórico eleitoral.
          </FeatureCard>
          <FeatureCard icon={DocumentMagnifyingGlassIcon} title="Declaração de Bens">
            Visualização acerca de bens declarados para cada candidato.
          </FeatureCard>
          <FeatureCard icon={MagnifyingGlassIcon} title="Informações de Campanha">
            Análise de receitas e despesas de campanha quando disponível.
          </FeatureCard>
          <FeatureCard icon={ChartBarSquareIcon} title="Estatísticas">
            Estatísticas e gráficos interativos para entender melhor o cenário eleitoral.
          </FeatureCard>
          <FeatureCard icon={ArrowDownOnSquareStackIcon} title="Dados Abertos">
            Os dados são acessíveis através do TSE e também disponibilizados em nosso repositório GitHub, garantindo transparência e confiabilidade.
          </FeatureCard>
        </div>
      </div>
    </section>
  );
};

export default FeaturesSection;
