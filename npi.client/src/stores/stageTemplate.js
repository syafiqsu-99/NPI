export const NPI_STAGES = {
  '0.0': {
    name: 'Enquiry',
    required: true,
    tasks: [
      { code: '0.1', title: 'Sales Enquiry Form', pic: 'Sales', hasLink: true },
      { code: '0.2', title: 'Customer Info', pic: 'Sales', hasLink: true },
      { code: '0.3', title: 'Project Awarded', pic: 'Technical', hasLink: false },
    ]
  },
  '1.0': {
    name: 'Project Start',
    required: true,
    tasks: [
      { code: '1.1', title: 'Project awarded / Contract signing', pic: 'Sales', hasLink: false },
      { code: '1.2', title: 'Drawing preparation', pic: 'Technical', hasLink: true },
      { code: '1.3', title: 'Drawing submission to customer', pic: 'Sales', hasLink: false },
      { code: '1.4', title: 'DFM preparation', pic: 'Technical', hasLink: true },
      { code: '1.5', title: 'DFM submission to customer', pic: 'Technical', hasLink: false },
      { code: '1.6', title: 'Customer drawing approval', pic: 'Sales', hasLink: true },
      { code: '1.7', title: 'PO issuance from customer', pic: 'Sales', hasLink: false },
      { code: '1.8', title: 'PO issuance to supplier', pic: 'Purchaser', hasLink: true },
    ]
  },
  '2.0': {
    name: 'Pilot Mould Fabrication',
    required: false,
    tasks: [
      { code: '2.1', title: 'Pilot mould fabrication', pic: 'Technical', hasLink: false },
      { code: '2.2', title: 'Mould trial + samples shipment', pic: 'Technical', hasLink: false },
      { code: '2.3', title: 'Trial sample verification', pic: 'Technical', hasLink: true },
      { code: '2.4', title: 'Mold modification (if any)', pic: 'Technical', hasLink: false },
      { code: '2.5', title: 'Mould trial + samples shipment', pic: 'Technical', hasLink: false },
      { code: '2.6', title: 'Trial sample verification', pic: 'Technical', hasLink: true },
      { code: '2.7', title: 'Discussion with customer', pic: 'Sales', hasLink: false },
      { code: '2.8', title: 'Samples submission to QA', pic: 'Technical', hasLink: false },
      { code: '2.9', title: 'FAI', pic: 'QA', hasLink: true },
      { code: '2.10', title: 'Samples submission to customer', pic: 'Sales', hasLink: false },
      { code: '2.11', title: 'Customer approval', pic: 'Sales', hasLink: true },
    ]
  },
  '3.0': {
    name: 'New Machine Purchase',
    required: false,
    tasks: [
      { code: '3.1', title: 'Machine fabrication', pic: 'Technical', hasLink: false },
      { code: '3.2', title: 'Packing of machine & mould', pic: 'Technical', hasLink: false },
      { code: '3.3', title: 'Machine delivery', pic: 'Technical', hasLink: false },
      { code: '3.4', title: 'Machine installation, test and commissioning', pic: 'Technical', hasLink: false },
    ]
  },
  '4.0': {
    name: 'Production Mould Fabrication',
    required: true,
    tasks: [
      { code: '4.1', title: 'MP mould fabrication', pic: 'Technical', hasLink: false },
      { code: '4.2', title: 'Mould trial + samples shipment', pic: 'Technical', hasLink: false },
      { code: '4.3', title: 'Trial sample verification', pic: 'Technical', hasLink: true },
      { code: '4.4', title: 'Mold modification (if any)', pic: 'Technical', hasLink: false },
      { code: '4.5', title: 'Mould trial + samples shipment', pic: 'Technical', hasLink: false },
      { code: '4.6', title: 'Trial sample verification', pic: 'Technical', hasLink: true },
      { code: '4.7', title: 'Discussion with customer', pic: 'Sales', hasLink: false },
      { code: '4.8', title: 'Samples submission to QA', pic: 'Technical', hasLink: false },
      { code: '4.9', title: 'FAI', pic: 'QA', hasLink: true },
      { code: '4.10', title: 'Samples submission to customer', pic: 'Sales', hasLink: false },
      { code: '4.11', title: 'Customer approval', pic: 'Sales', hasLink: true },
    ]
  },
  '5.0': {
    name: 'Trial Run at JJ',
    required: true,
    tasks: [
      { code: '5.1', title: 'Planning for trial', pic: 'Production', hasLink: true, docFormat: 'Excel' },
      { code: '5.2', title: 'Mould trial, T0', pic: 'Production', hasLink: false },
      { code: '5.3', title: 'Trial sample verification', pic: 'Technical', hasLink: true, docFormat: 'Excel' },
      { code: '5.4', title: 'Mold modification (if any)', pic: 'Technical', hasLink: false },
      { code: '5.5', title: 'Mould trial, T1', pic: 'Technical', hasLink: false },
      { code: '5.6', title: 'Blue Card', pic: 'Technical', hasLink: true, docFormat: 'Excel' },
      { code: '5.7', title: 'Trial sample verification', pic: 'Technical', hasLink: true, docFormat: 'Excel' },
      { code: '5.8', title: 'FAI', pic: 'QA', hasLink: true, docFormat: 'Excel', roleGated: 'QA' },
      { code: '5.9', title: 'Samples submission to customer', pic: 'Sales', hasLink: false },
      { code: '5.10', title: 'Customer approval', pic: 'Sales', hasLink: true, docFormat: 'Excel/Email/PDF' },
    ]
  }
}
