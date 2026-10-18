import React from 'react';
import type { ActiveTab } from '../types';


interface HeaderSectionProps {
  activeTab: ActiveTab;
  vehicleCount: number;
  categoryCount: number;
  onSelectTab: (tab: ActiveTab) => void;
}


export const HeaderSection: React.FC<HeaderSectionProps> = ({
  activeTab,
  vehicleCount,
  categoryCount,
  onSelectTab,
}) => {
  return (
    <header className="header-section">
      <div className="header-branding">
        <h1 className="header-title">Motorvehicle &amp; Weight Category Manager</h1>
        <p className="header-subtitle">
          Dynamic vehicle weight categorisation &amp; contiguous category administration
        </p>
      </div>
      <nav className="header-tab-nav" aria-label="Main navigation">
        <button
          type="button"
          className={activeTab === 'vehicles' ? 'tab-btn active' : 'tab-btn'}
          onClick={() => onSelectTab('vehicles')}
        >
          Vehicles ({vehicleCount})
        </button>
        <button
          type="button"
          className={activeTab === 'categories' ? 'tab-btn active' : 'tab-btn'}
          onClick={() => onSelectTab('categories')}
        >
          Category Administration ({categoryCount})
        </button>
      </nav>
    </header>
  );
};
