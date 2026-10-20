import React from 'react';
import { ArrowDownOnSquareStackIcon, ChartBarSquareIcon, DocumentMagnifyingGlassIcon, DocumentTextIcon, IdentificationIcon, MagnifyingGlassIcon } from '@heroicons/react/24/outline';
import Card from '../shared/Card';
import SectionHeading from '../shared/SectionHeading';

const FeatureCard: React.FC<{ icon: React.ElementType, title: string, children: React.ReactNode }> = ({ icon: Icon, title, children }) => {
  return (
    <Card hasAnimation className="h-full text-left">
      <span className="mb-5 flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br from-indigo-500/25 to-violet-500/15 text-indigo-200 ring-1 ring-inset ring-indigo-300/20">
        <Icon className="h-6 w-6" aria-hidden="true" />
      </span>
      <h3 className="mb-2 text-lg font-semibold text-white">{title}</h3>
      <p className="text-sm leading-relaxed text-slate-400">{children}</p>
    </Card>
  );
};

const FeaturesSection: React.FC = () => {
  return (
    <section id="recursos" className="pb-12 pt-8 md:pb-20">
      <div className="mx-auto max-w-7xl px-4 sm:px-6">
        <SectionHeading
          eyebrow="Recursos"
          title="Tudo o que o TSE publica, em um só lugar"
          subtitle="Perfis, patrimônio, campanhas e estatísticas — organizados para consulta rápida."
          className="mb-12"
        />
        <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
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
