import React from 'react';


interface StatusBannerSectionProps {
  errorBanner: string | null;
  successBanner: string | null;
  onDismiss: () => void;
}


export const StatusBannerSection: React.FC<StatusBannerSectionProps> = ({
  errorBanner,
  successBanner,
  onDismiss,
}) => {
  if (!errorBanner && !successBanner) {
    return null;
  }


  return (
    <section className="status-banner-section" aria-live="polite">
      {errorBanner && (
        <div className="alert alert-error">
          <span className="alert-text">{errorBanner}</span>
          <button type="button" className="alert-dismiss-btn" onClick={onDismiss}>
            Dismiss
          </button>
        </div>
      )}
      {successBanner && (
        <div className="alert alert-success">
          <span className="alert-text">{successBanner}</span>
          <button type="button" className="alert-dismiss-btn" onClick={onDismiss}>
            Dismiss
          </button>
        </div>
      )}
    </section>
  );
};
