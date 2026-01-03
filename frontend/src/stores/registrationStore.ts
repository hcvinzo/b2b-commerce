import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { ContactPerson, BusinessInfo, OperationalDetails, BankingDocuments } from '@/types'

interface RegistrationState {
  currentStep: number
  contactPerson: Partial<ContactPerson>
  businessInfo: Partial<BusinessInfo>
  operationalDetails: Partial<OperationalDetails>
  bankingDocuments: Partial<BankingDocuments>

  setCurrentStep: (step: number) => void
  setContactPerson: (data: Partial<ContactPerson>) => void
  setBusinessInfo: (data: Partial<BusinessInfo>) => void
  setOperationalDetails: (data: Partial<OperationalDetails>) => void
  setBankingDocuments: (data: Partial<BankingDocuments>) => void
  reset: () => void
}

const initialState = {
  currentStep: 1,
  contactPerson: {},
  businessInfo: {
    authorizedPersons: [
      { ad_soyad: '', kimlik_no: '', ortaklik_payi: 0 },
    ],
  },
  operationalDetails: {
    calisanSayisi: '',
    isletmeYapisi: '',
    ciro: [{}],
    musteriKitlesi: [{}],
    isOrtaklari: [{}],
    satilanUrunKategorileri: [],
    calismaKosullari: [],
  },
  bankingDocuments: {
    bankAccounts: [
      { bankName: '', iban: '' },
    ],
    collaterals: [
      { type: '', amount: 0, currency: 'TRY' as const },
    ],
    documents: {},
  },
}

export const useRegistrationStore = create<RegistrationState>()(
  persist(
    (set) => ({
      ...initialState,
      setCurrentStep: (step) => set({ currentStep: step }),
      setContactPerson: (data) => set((state) => ({
        contactPerson: { ...state.contactPerson, ...data }
      })),
      setBusinessInfo: (data) => set((state) => ({
        businessInfo: { ...state.businessInfo, ...data }
      })),
      setOperationalDetails: (data) => set((state) => ({
        operationalDetails: { ...state.operationalDetails, ...data }
      })),
      setBankingDocuments: (data) => set((state) => ({
        bankingDocuments: { ...state.bankingDocuments, ...data }
      })),
      reset: () => set(initialState),
    }),
    {
      name: 'registration-storage',
    }
  )
)
