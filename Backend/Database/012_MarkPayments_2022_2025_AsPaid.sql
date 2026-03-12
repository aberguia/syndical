-- Script pour créer et marquer tous les paiements de 2022 à 2025 comme payés
-- Date: 2026-03-12
-- Les paiements mensuels sont créés pour chaque appartement actif
-- Puis marqués comme payés (IsPaid = 1)

-- Désactiver le mode safe update pour les WHERE sans clé primaire
SET SQL_SAFE_UPDATES = 0;

-- 1. Insérer tous les paiements mensuels manquants pour 2022-2025
-- Pour chaque appartement actif et chaque mois des années 2022-2025
INSERT IGNORE INTO MonthlyPayments (ApartmentId, Year, Month, Amount, PaymentDate, RecordedById, RecordedAt, IsPaid)
SELECT 
    a.Id as ApartmentId,
    ym.Year,
    ym.Month,
    500.00 as Amount,  -- Montant par défaut (à adapter selon tes charges réelles)
    LAST_DAY(STR_TO_DATE(CONCAT(ym.Year, '-', LPAD(ym.Month, 2, '0'), '-01'), '%Y-%m-%d')) as PaymentDate,
    1 as RecordedById,  -- Utilisateur administrateur
    NOW() as RecordedAt,
    1 as IsPaid  -- Payé directement
FROM 
    Apartments a
    CROSS JOIN (
        SELECT 2022 as Year, 1 as Month UNION SELECT 2022, 2 UNION SELECT 2022, 3 UNION SELECT 2022, 4
        UNION SELECT 2022, 5 UNION SELECT 2022, 6 UNION SELECT 2022, 7 UNION SELECT 2022, 8
        UNION SELECT 2022, 9 UNION SELECT 2022, 10 UNION SELECT 2022, 11 UNION SELECT 2022, 12
        UNION SELECT 2023, 1 UNION SELECT 2023, 2 UNION SELECT 2023, 3 UNION SELECT 2023, 4
        UNION SELECT 2023, 5 UNION SELECT 2023, 6 UNION SELECT 2023, 7 UNION SELECT 2023, 8
        UNION SELECT 2023, 9 UNION SELECT 2023, 10 UNION SELECT 2023, 11 UNION SELECT 2023, 12
        UNION SELECT 2024, 1 UNION SELECT 2024, 2 UNION SELECT 2024, 3 UNION SELECT 2024, 4
        UNION SELECT 2024, 5 UNION SELECT 2024, 6 UNION SELECT 2024, 7 UNION SELECT 2024, 8
        UNION SELECT 2024, 9 UNION SELECT 2024, 10 UNION SELECT 2024, 11 UNION SELECT 2024, 12
        UNION SELECT 2025, 1 UNION SELECT 2025, 2 UNION SELECT 2025, 3 UNION SELECT 2025, 4
        UNION SELECT 2025, 5 UNION SELECT 2025, 6 UNION SELECT 2025, 7 UNION SELECT 2025, 8
        UNION SELECT 2025, 9 UNION SELECT 2025, 10 UNION SELECT 2025, 11 UNION SELECT 2025, 12
    ) ym
WHERE 
    a.IsActive = 1
    AND a.IsDeleted = 0
    AND NOT EXISTS (
        SELECT 1 FROM MonthlyPayments mp 
        WHERE mp.ApartmentId = a.Id 
        AND mp.Year = ym.Year 
        AND mp.Month = ym.Month
    );

-- 2. Marquer tous les paiements de 2022 à 2025 comme payés
UPDATE MonthlyPayments
SET IsPaid = 1
WHERE Year IN (2022, 2023, 2024, 2025);

-- 3. Vérification: afficher le résumé par année
SELECT 
    Year,
    COUNT(*) as 'Nombre de paiements',
    SUM(Amount) as 'Montant total',
    SUM(IF(IsPaid = 1, 1, 0)) as 'Nombre payés',
    SUM(IF(IsPaid = 1, Amount, 0)) as 'Montant payé'
FROM MonthlyPayments
WHERE Year IN (2022, 2023, 2024, 2025)
GROUP BY Year
ORDER BY Year DESC;

-- Réactiver le mode safe update
SET SQL_SAFE_UPDATES = 1;
