using System;

namespace CreditCardApplications
{
    public class CreditCardApplicationEvaluator
    {
        private const int AutoReferralMaxAge = 20;
        private const int HighIncomeThreshold = 100_000;
        private const int LowIncomeThreshold = 20_000;

        private readonly IFrequentFlyerNumberValidator _validator;
        private readonly FraudLookup _fraudLookup;

        public int ValidatorLookupCount { get; private set; }

        public CreditCardApplicationEvaluator(IFrequentFlyerNumberValidator validator, FraudLookup fraudLookup = null)
        {
            _validator = validator;
            _validator.ValidaorLookupPerformed += ValidaorLookupPerformed;
            _fraudLookup = fraudLookup;
        }

        private void ValidaorLookupPerformed(object sender, EventArgs e)
        {
            ValidatorLookupCount++;
        }

        public CreditCardApplicationDecision Evaluate(CreditCardApplication application)
        {
            if (_fraudLookup != null && _fraudLookup.IsFraudRisk(application))
            {
                return CreditCardApplicationDecision.ReferredToHumanFraudRisk;
            }

            if (application.GrossAnnualIncome >= HighIncomeThreshold)
            {
                return CreditCardApplicationDecision.AutoAccepted;
            }

            if (_validator.ServiceInformation.License.LicenseKey == "EXPIRED")
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            _validator.ValidationMode = application.Age >= 30 ? ValidationMode.Detailed : ValidationMode.Quick;

            bool isValidFrequentyFlyerNumber;

            try
            {
                isValidFrequentyFlyerNumber = _validator.IsValid(application.FrequentFlyerNumber);
            }
            catch (Exception)
            {
                // log
                return CreditCardApplicationDecision.ReferredToHuman;
            }


            if (!isValidFrequentyFlyerNumber)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            if (application.Age <= AutoReferralMaxAge)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            if (application.GrossAnnualIncome < LowIncomeThreshold)
            {
                return CreditCardApplicationDecision.AutoDeclined;
            }

            return CreditCardApplicationDecision.ReferredToHuman;
        }
    }
}
