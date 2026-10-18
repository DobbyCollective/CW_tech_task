import React, { useState } from 'react';
import type { CreateVehicleRequest, ManufacturerRecord } from '../types';


interface AddVehicleSectionProps {
  manufacturers: ManufacturerRecord[];
  onCreateVehicle: (payload: CreateVehicleRequest) => Promise<boolean>;
}


export const AddVehicleSection: React.FC<AddVehicleSectionProps> = ({
  manufacturers,
  onCreateVehicle,
}) => {
  const [isOrganisationOwner, setIsOrganisationOwner] = useState<boolean>(false);
  const [ownerFirstName, setOwnerFirstName] = useState<string>('');
  const [ownerLastName, setOwnerLastName] = useState<string>('');
  const [ownerOrganisation, setOwnerOrganisation] = useState<string>('');
  const [selectedManufacturerKey, setSelectedManufacturerKey] = useState<number>(0);
  const [manufacturedYear, setManufacturedYear] = useState<string>(
    new Date().getFullYear().toString()
  );
  const [vehicleWeight, setVehicleWeight] = useState<string>('');


  const effectiveManufacturerKey: number =
    selectedManufacturerKey !== 0
      ? selectedManufacturerKey
      : manufacturers.length > 0
        ? manufacturers[0].manufacturerKey
        : 0;


  const handleSubmit = async (e: React.FormEvent): Promise<void> => {
    e.preventDefault();


    const payload: CreateVehicleRequest = {
      ownerFirstName: isOrganisationOwner ? '' : ownerFirstName.trim(),
      ownerLastName: isOrganisationOwner ? '' : ownerLastName.trim(),
      ownerOrganisation: isOrganisationOwner ? ownerOrganisation.trim() : '',
      manufacturerKey: effectiveManufacturerKey,
      manufacturedYear: Number(manufacturedYear),
      vehicleWeight: Number(vehicleWeight),
    };


    const created: boolean = await onCreateVehicle(payload);
    if (created) {
      setOwnerFirstName('');
      setOwnerLastName('');
      setOwnerOrganisation('');
      setVehicleWeight('');
    }
  };


  return (
    <section className="panel-card vehicle-form-section">
      <h2 className="section-title">Add New Vehicle</h2>
      <form onSubmit={(e) => void handleSubmit(e)} className="form-flex-stack">
        <label className="checkbox-row">
          <input
            type="checkbox"
            checked={isOrganisationOwner}
            onChange={(e) => setIsOrganisationOwner(e.target.checked)}
          />
          <span>Owner is an Organisation (First &amp; Last Name not required)</span>
        </label>


        {isOrganisationOwner ? (
          <div className="field-group">
            <label htmlFor="ownerOrganisation">Organisation Name *</label>
            <input
              id="ownerOrganisation"
              type="text"
              value={ownerOrganisation}
              onChange={(e) => setOwnerOrganisation(e.target.value)}
              placeholder="e.g. CreditWorks NZ Ltd"
              required
            />
          </div>
        ) : (
          <div className="field-grid-two-col">
            <div className="field-group">
              <label htmlFor="ownerFirstName">Owner First Name *</label>
              <input
                id="ownerFirstName"
                type="text"
                value={ownerFirstName}
                onChange={(e) => setOwnerFirstName(e.target.value)}
                placeholder="e.g. Sam"
                required
              />
            </div>
            <div className="field-group">
              <label htmlFor="ownerLastName">Owner Last Name *</label>
              <input
                id="ownerLastName"
                type="text"
                value={ownerLastName}
                onChange={(e) => setOwnerLastName(e.target.value)}
                placeholder="e.g. Smith"
                required
              />
            </div>
          </div>
        )}


        <div className="field-group">
          <label htmlFor="manufacturer">Manufacturer *</label>
          <select
            id="manufacturer"
            value={effectiveManufacturerKey}
            onChange={(e) => setSelectedManufacturerKey(Number(e.target.value))}
            required
          >
            {manufacturers.map((m: ManufacturerRecord) => (
              <option key={m.manufacturerKey} value={m.manufacturerKey}>
                {m.manufacturerName}
              </option>
            ))}
          </select>
        </div>


        <div className="field-grid-two-col">
          <div className="field-group">
            <label htmlFor="year">Year of Manufacture *</label>
            <input
              id="year"
              type="number"
              min={1886}
              max={new Date().getFullYear() + 1}
              value={manufacturedYear}
              onChange={(e) => setManufacturedYear(e.target.value)}
              required
            />
          </div>
          <div className="field-group">
            <label htmlFor="weight">Weight (kg, max. 2 decimals) *</label>
            <input
              id="weight"
              type="number"
              step="0.01"
              min="0.01"
              value={vehicleWeight}
              onChange={(e) => setVehicleWeight(e.target.value)}
              placeholder="e.g. 1500.10"
              required
            />
          </div>
        </div>


        <button type="submit" className="btn-primary">
          Add Vehicle
        </button>
      </form>
    </section>
  );
};
