-- Insert test wallet (Vitalik's public address) for Moralis sync testing
INSERT INTO dash."CustodyWallets"
(
    "Id",
    "WalletAddress",
    "Nickname",
    "WalletType",
    "SupportedChains",
    "Status",
    "CreatedAt",
    "UpdatedAt"
)
VALUES
(
    gen_random_uuid(),
    '0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045',  -- Vitalik's public address
    'Vitalik Test Wallet',
    'EOA',
    ARRAY['eth', 'polygon', 'bsc'],
    'Active',
    NOW(),
    NOW()
)
ON CONFLICT ("WalletAddress") DO NOTHING
RETURNING "Id", "WalletAddress", "Nickname";
