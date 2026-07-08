/*
  Script base para el sink SQL de Serilog en PostgreSQL.
  Ejecutar una sola vez en la base de datos destino antes de habilitar escritura en SQL.
*/

CREATE TABLE IF NOT EXISTS public.application_logs
(
  id BIGSERIAL PRIMARY KEY,
  message TEXT NULL,
  message_template TEXT NULL,
  level VARCHAR(128) NULL,
  raise_date TIMESTAMPTZ NOT NULL,
  exception TEXT NULL,
  properties JSONB NULL,
  token TEXT NULL,
  tipo_transaccion TEXT NULL,
  metodo TEXT NULL,
  capa TEXT NULL
);

CREATE INDEX IF NOT EXISTS ix_application_logs_raise_date
  ON public.application_logs (raise_date DESC);

CREATE INDEX IF NOT EXISTS ix_application_logs_level
  ON public.application_logs (level);
